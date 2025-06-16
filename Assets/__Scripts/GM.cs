using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static EventSender;

public class GM : MonoBehaviour
{
    // This class contains the main game code.
    
    public static GM inst { get; private set; }
    
    // Geometry and playfield
    public Vector3 offset2D;
    public float ballSpeedCap;
    public List<string> collisionLayers;
    private CameraController _cameraController;
    public BallDispenser ballDispenser;
    public DotMatrixDisplay dotMatrixDisplay;
    
    // UI
    public Countdown countdown;
    public GameOver gameOverLose;
    public GameOver gameOverWin;
    public TouchCanvas touchCanvas;
    
    // Game state
    public GameMode mode { get; private set; }
    public UnityEvent modeChangeEvent { get; private set; } = new UnityEvent();
    public bool gamePaused { get; private set; }
    public UnityEvent<bool> pauseEvent { get; private set; }
    
    // Scoring
    private int _currentScore = 0;
    private int _targetScore = 0;
    private int _prevTargetScore;
    private int _targetScoreBase = 8000;
    
    private int _level;
    private int _totalLevels = 9;
    public Color[] levelColors = new Color[10];
    
    // Coins
    public int coinCount { get; set; }
    private bool _magneticCoins;            // Shop upgrade
    private int initialCoinPointValue = 50;
    public int coinPointValue;
    
    // Locked balls and multiball
    private int _ballsLocked;
    private int _locksNeededForMultiball = 3;
    public UnityEvent multiballEndEvent { get; private set; } = new UnityEvent();
        
    // Score multipliers
    private int _permaMult;
    private int _ballMultLevel = 1;
    private float _ballMultTimer = 0f;
    private float _ballMultTimerMax = 60f;
    private float _ballMultDecayRate = 1f;
    private int _ballMultMaxedOutBonus = 5000;
    
    // Nudging
    private float _nudgeStrength = 1.5f;      // Shop upgrade
    private float _initialNudgeCooldown = .5f;    // Shop upgrade
    private float _nudgeCooldown;
    public Image nudgeCooldownImage;
    
    // Audio
    public AudioSource audioSource;
    public AudioSource audioSourceNoReverb;
    public AudioSource musicSource;
    public SoundClipCollection soundClips = new SoundClipCollection();
    
    // Debug
    public ScoreLog scoreLog;


    void Awake()
    {
        inst = this;

        pauseEvent = new UnityEvent<bool>();
        _cameraController = FindObjectOfType<CameraController>();
        
        // Screen.autorotateToPortrait = false;
        // Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.AutoRotation;
#if UNITY_EDITOR
        musicSource.mute = true;     
#endif
    }

    void Start()
    {
        EventSender[] senders = FindObjectsByType<EventSender>(FindObjectsSortMode.None);
        foreach (EventSender sender in senders)
        {
            try
            {
                sender.GetBoardEvent().AddListener(HandleBoardEvent);
            }
            catch 
            {
                Debug.LogError("Uninitialized event with " + sender.name);
            }
            
        }
        DeathBarrier[] deathBarriers = FindObjectsByType<DeathBarrier>(FindObjectsSortMode.None);
        foreach (DeathBarrier db in deathBarriers)
        {
            db.GetBoardEvent().AddListener(OnDrain);
        }
        
        Application.targetFrameRate = -1;
        Time.timeScale = 1f;
        SetGameMode(GameMode.Title);
    }
    
    void Update()
    {
        if (InputMan.inst.musicTogglePressed)
        {
            musicSource.mute = !musicSource.mute;
            touchCanvas.SetMusicIconState(musicSource.mute);
        }

        if (InputMan.inst.fullscreenPressed)
            Screen.fullScreen = !Screen.fullScreen;
        
        
        if (mode == GameMode.Shop) 
            return;
        
        if (gamePaused)
            return;
        
        
        if (_ballMultLevel > 0)
        {
            _ballMultTimer -= Time.deltaTime * _ballMultDecayRate;
            if (_ballMultTimer < 0f)
            {
                _ballMultLevel--;
                _ballMultTimer = _ballMultTimerMax;
                ballDispenser.SetBallMaterial(_ballMultLevel);
            }
        }
        
        Vector2 nudgeVector = Vector2.zero;
        if (_nudgeCooldown > 0f)
        {
            _nudgeCooldown -= Time.deltaTime;
            if (_nudgeCooldown <= 0f)
            {
                nudgeCooldownImage.gameObject.SetActive(false);
            }
            else
            {
                nudgeCooldownImage.material.SetFloat("_Percentage", Mathf.Clamp01(_nudgeCooldown / _initialNudgeCooldown));
            }
        }
        else
        {
            if (InputMan.inst.leftNudgePressed)
            {
                nudgeVector = Vector2.left;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
                _cameraController.Nudge(nudgeVector);
            }
            else if (InputMan.inst.rightNudgePressed)
            {
                nudgeVector = Vector2.right;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
                _cameraController.Nudge(nudgeVector);
            }
            else if (InputMan.inst.upNudgePressed)
            {
                nudgeVector = Vector2.up;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
                _cameraController.Nudge(nudgeVector);
            }

            foreach (GameObject ball in ballDispenser.activeBalls)
            {
                ball.GetComponent<Ball>().AddImpulseForce(nudgeVector * (50f * _nudgeStrength));
            }
        }
    }

    void HandleBoardEvent(EventInfo info)
    {
        // Receives and handles events from all EventSender objects that existed at startup.
        try
        {
            switch (info.Type)
            {
                case EventSender.EventType.Trigger:
                    break;
                case EventSender.EventType.AddCoins:
                    coinCount += (int)info.Data;
                    break;
                case EventSender.EventType.AddPoints:
                    AddScore((int)info.Data, true, info);
                    break;
                case EventSender.EventType.AddPointsNoMult:
                    AddScore((int)info.Data, false, info);
                    break;
                case EventSender.EventType.AddSeconds:
                    GameTime.inst.AddSubTime((int)info.Data);
                    break;
                case EventSender.EventType.AddBallMult:
                    if (_ballMultLevel == 4)
                    {
                        AddScore(_ballMultMaxedOutBonus, false, null);
                        _ballMultTimer = _ballMultTimerMax;
                    }
                    else
                    {
                        _ballMultLevel++;
                        ballDispenser.SetBallMaterial(_ballMultLevel);
                    }

                    _ballMultTimer = _ballMultTimerMax;
                    break;
                case EventSender.EventType.PlaySound:
                    PlaySound(info.Data, true);
                    break;
                case EventSender.EventType.PlaySoundNoReverb:
                    PlaySound(info.Data, false);
                    break;
                case EventSender.EventType.LockBall:
                    Ball ball = info.Data as Ball;
                    if (ball)
                    {
                        ballDispenser.DespawnBall(ball.gameObject);
                        _ballsLocked++;
                        if (_ballsLocked == _locksNeededForMultiball)
                        {
                            _ballsLocked = 0;
                            BallSaver.inst.AddTime(15);
                            StartMultiball();
                        }
                        else
                        {
                            // TODO: make this less hardcoded
                            int ballsLeft = _locksNeededForMultiball - _ballsLocked;
                            string ballsLeftTexString = "";
                            if (ballsLeft == 1)
                            {
                                ballsLeftTexString = "1More";
                            }
                            else if (ballsLeft == 2)
                            {
                                ballsLeftTexString = "2More";
                            }

                            DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
                                new DotMatrixDisplay.DmdAnim[]
                                {
                                    new(DotMatrixDisplay.AnimType.ScrollInOut,
                                        DotMatrixDisplay.AnimOrient.Vertical,
                                        .75f, 0, "Ball", TextureWrapMode.Clamp),
                                    new(DotMatrixDisplay.AnimType.ScrollInOut,
                                        DotMatrixDisplay.AnimOrient.Vertical,
                                        .75f, 0, "Locked", TextureWrapMode.Clamp),

                                    new(DotMatrixDisplay.AnimType.Hold,
                                        DotMatrixDisplay.AnimOrient.Horizontal,
                                        1f, 0, ballsLeftTexString, TextureWrapMode.Clamp),
                                    new(DotMatrixDisplay.AnimType.Hold,
                                        DotMatrixDisplay.AnimOrient.Horizontal,
                                        1f, 0, "Needed", TextureWrapMode.Clamp),
                                }
                            );
                            dotMatrixDisplay.AddMessage(message);
                            ballDispenser.SpawnBall();
                        }
                    }
                    else
                    {
                        Debug.LogError("Attempted to lock non-Ball object: " + info.Data);
                    }

                    break;
                case EventSender.EventType.ShowMessage:
                    if (!dotMatrixDisplay)
                        break;
                    if (info.Data is DotMatrixDisplay.Message messageInfo)
                    {
                        dotMatrixDisplay.AddMessage(messageInfo);
                        break;
                    }

                    Debug.LogError("Couldn't show message due to no DmdMessage object");

                    break;
                case EventSender.EventType.SpawnBall:
                    ballDispenser.SpawnBall();
                    break;
                case EventSender.EventType.AddSaver:
                    BallSaver.inst.AddTime((int)info.Data);
                    break;
                default:
                    Debug.LogError("Unhandled event type:" + info.Type);
                    break;
            }
        }
        catch
        {
            Debug.LogError("Error handling " + info.Type + " from " + info.Sender.name);
        }
    }

    public void PlaySound(object sound, bool reverb=true)
    {
        var source = reverb ? audioSource : audioSourceNoReverb;
        var clip = sound as AudioClip;
        if (clip)
        {
            source.PlayOneShot(clip);
            return;
        }

        if ((string)sound == "")
            return;
        source.PlayOneShot(soundClips.Get((string)sound).obj);
    }

    void StartMultiball()
    {
        if (mode != GameMode.Play)
        {
            Debug.LogError("Attempted to start multiball from non-Play mode");
            return;
        }

        multiballEndEvent = new UnityEvent();
        SetGameMode(GameMode.Multiball);
        
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollIn, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .25f, 4, "Multiball", TextureWrapMode.Repeat),
                new (DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    1f, 0, "Multiball", TextureWrapMode.Repeat),
            }
        );
    }
    
    private void OnDrain(EventInfo info)
    {
        GameObject ball = info.Data as GameObject;
        ballDispenser.DespawnBall(ball);
        PlaySound("latch_clunk");
        
        if (GameTime.inst.timeRemaining <= 0f)
        {
            SetGameMode(GameMode.GameOver);
        }
        else if (BallSaver.inst.timeLeft <= 0f)
        {
            if (ballDispenser.ballsInPlay == 0) 
            {
                GameTime.inst.AddSubTime(-10);
                if (GameTime.inst.timeRemaining <= 10f)
                {
                    SetGameMode(GameMode.GameOver);
                }
                ballDispenser.SpawnBall();
            }
        }
        else
        {
            PlaySound("ball_saved", false);
            dotMatrixDisplay.AddMessage(new DotMatrixDisplay.Message(new[]
            {
                new DotMatrixDisplay.DmdAnim(DotMatrixDisplay.AnimType.ScrollInOut,
                    DotMatrixDisplay.AnimOrient.Horizontal,
                    1f, 0, "BallSaved", TextureWrapMode.Clamp)
            }));
            ballDispenser.SpawnBall();
        }
    }
    
    public void SetGameMode(GameMode newMode)
    {
        if (mode == newMode)
        {
            return;
        }

        if (newMode == GameMode.Title)
        {
            FindAnyObjectByType<TitleScreen>().gameObject.SetActive(true);   
        }
        else if (newMode == GameMode.Countdown)
        {
            countdown.gameObject.SetActive(true);
            countdown.StartCountdown();
        }
        else if (mode == GameMode.Countdown && newMode == GameMode.Play)
        {
            ballDispenser.SpawnBall();
            StartNextLevel();
        }
        else if (newMode == GameMode.GameOver)
        {
            gameOverLose.gameObject.SetActive(true);
            gameOverLose.scoreText.text = "Final Score: " + _currentScore + "\nPress any key to restart";
        }
        
        mode = newMode;
        modeChangeEvent.Invoke();
    }
    
    public void StartNewGame()
    {
        MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in monoBehaviours)
        {
            INeedReset[] inrs = mb.GetComponents<INeedReset>();
            if (inrs != null)
            {
                foreach (INeedReset inr in inrs)
                    inr.ResetForNewGame();
            }
        }
        
        _level = 0;
        _currentScore = 0;
        _targetScore = 0;
        _prevTargetScore = 0;
        
        
        
        dotMatrixDisplay.UpdateScoreTex(_currentScore, 0f);
        
        _ballsLocked = 0;
        coinPointValue = initialCoinPointValue;
        coinCount = 0;
        GameTime.inst.NextLevel();
        BallSaver.inst.StartNewGame();
        
        SetGameMode(GameMode.Countdown);
    }
    
    void StartNextLevel()
    {
        if (_level != 0)
        {
            audioSourceNoReverb.PlayOneShot(soundClips.Get("round_complete").obj);
            
            DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
                new DotMatrixDisplay.DmdAnim[]
                {
                    new (DotMatrixDisplay.AnimType.ScrollInOut, 
                        DotMatrixDisplay.AnimOrient.Horizontal,
                        2f, 0, "Level", TextureWrapMode.Clamp),
                    new(DotMatrixDisplay.AnimType.ScrollInOut, 
                        DotMatrixDisplay.AnimOrient.Horizontal,
                        2f, 0, "Complete", TextureWrapMode.Clamp) },
                true, false
            );
            dotMatrixDisplay.AddMessage(message);
            GameTime.inst.NextLevel();
        }
        
        _level++;
        if (_level == _totalLevels + 1)
        {
            SetGameMode(GameMode.GameOver);
            gameOverWin.gameObject.SetActive(true);
            gameOverWin.scoreText.text = "Final Score: " + _currentScore + "\nPress any key to restart";
        }
        else
        {
            _prevTargetScore = _targetScore;
            _targetScore = (int)(_targetScoreBase * _level * (1f + .25f * (_level - 1))); 
            dotMatrixDisplay.UpdateTargetScoreTex(_targetScore);
            dotMatrixDisplay.UpdateLevelTex(_level, _totalLevels);
            if (scoreLog)
            {
                scoreLog.NextRound();
            }
        }

    }
    
    void AddScore(int score, bool useMult, EventInfo info = null)
    {
        float multVal = 1f;
        if (useMult)
        {
            multVal = 1f + _ballMultLevel * .5f;
            multVal *= 1f + (.1f * _permaMult);
        }
        _currentScore += (int)(score * multVal);
        
        if (_currentScore >= _targetScore)
        {
            StartNextLevel();
        }

        if (info != null && scoreLog)
        {
            string tag = "";
            if (info.Sender)
            {
                tag = info.Sender.tag;
            }
            scoreLog.AddScore(tag, (int)(score * multVal));
        }
        
        int numerator = _currentScore - _prevTargetScore;
        int denominator = _targetScore - _prevTargetScore;
        float percentage = (float)numerator / denominator;

        dotMatrixDisplay.UpdateScoreTex(_currentScore, percentage);
    }
    
    public void ApplyShopItemEffect(Shop.ItemEffect effect, int value)
    {
        switch (effect)
        {
            case Shop.ItemEffect.UpgradeChutes:
                
                UpgradableChute[] toUpgrade = GameObject.FindObjectsByType<UpgradableChute>(FindObjectsSortMode.None);
                foreach (var chute in toUpgrade)
                {
                    chute.Upgrade(value);
                }
                break;
            case Shop.ItemEffect.UpgradeBumpers:
                JetBumperMan[] bumpersToUpgrade = GameObject.FindObjectsByType<JetBumperMan>(FindObjectsSortMode.None);
                foreach (var bumper in bumpersToUpgrade)
                {
                    bumper.Upgrade(value);
                }
                break;
            case Shop.ItemEffect.UpgradeDropTargets:
                DropTargetMan[] dropsToUpgrade = GameObject.FindObjectsByType<DropTargetMan>(FindObjectsSortMode.None);
                foreach (var drop in dropsToUpgrade)
                {
                    drop.Upgrade(value);
                }
                break;
            case Shop.ItemEffect.UpgradeSpinners:
                Spinner[] spinners = GameObject.FindObjectsByType<Spinner>(FindObjectsSortMode.None);
                foreach (var spinner in spinners)
                {
                    spinner.Upgrade(value);
                }
                break;
            case Shop.ItemEffect.IncreaseCoinSpawn:
                CoinSpawner[] coinSpawners = GameObject.FindObjectsByType<CoinSpawner>(FindObjectsSortMode.None);
                foreach (var spawner in coinSpawners)
                {
                    spawner.SetSpawnInterval(spawner.GetSpawnInterval() - value);
                }
                break;
            case Shop.ItemEffect.AddPermanentMult:
                _permaMult += value;
                break;
            case Shop.ItemEffect.AddPermanentTime:
                GameTime.inst.AddPermaTimeBonus(value);
                break;
            case Shop.ItemEffect.ReduceMultDecay:
                _ballMultDecayRate -= value * .1f;
                if (_ballMultDecayRate < 0f)
                {
                    _ballMultDecayRate = .1f;
                }
                break;
            case Shop.ItemEffect.IncreaseNudgeStrength:
                _nudgeStrength += value * .1f;
                break;
            case Shop.ItemEffect.DecreaseNudgeCooldown:
                // TODO
                break;
            case Shop.ItemEffect.CoinsGivePoints:
                coinPointValue += value;
                break;
            case Shop.ItemEffect.DisableLetterToggle:
                ToggleableLetters[] tLetters = GameObject.FindObjectsByType<ToggleableLetters>(FindObjectsSortMode.None);
                foreach (var tl in tLetters)
                {
                    tl.canBeToggledOff = false;
                }
                break;
            case Shop.ItemEffect.MagneticCoins:
                // TODO
                break;
            case Shop.ItemEffect.IncreaseSlotWinrate:
                break;
        }
    }
    
    public enum GameMode {
        Title, Countdown, Play, Multiball, Shop, GameOver
    }

    [System.Serializable]
    public class SoundClipCollection: DictCollection<AudioClip>
    {

    }
}


