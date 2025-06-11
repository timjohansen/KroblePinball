using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static EventSender;

public class GM : MonoBehaviour
{

    public static GM inst;
    public bool paused;
    public List<string> allLayers;


    public Vector3 offset2D;
    public GameObject simpleTriggerPrefab;
    public Board board;
    public BallDispenser ballDispenser;
    public DotMatrixDisplay dotMatrixDisplay;
    
    public float ballSpeedCap;
    public UnityEvent modeChangeEvent { get; private set; } = new UnityEvent();
    UnityEvent<bool> _pauseEvent;
    public GameMode mode { get; private set; }
    
    public Countdown countdown;
    public GameOver gameOverLose;
    public GameOver gameOverWin;
    public UnityEvent multiballEndEvent { get; private set; } = new UnityEvent();
    
    public MaterialSwapper saverGraphic;
    
    // public Material[] levelMats = new Material[10];
    public Color[] levelColors = new Color[10];
    
    private int _currentScore = 0;
    private int _targetScore = 0;
    private int _prevTargetScore;
    private int _targetScoreBase = 8000;
    
    private int _round;
    private int _totalRounds = 9;
    public float timeRemaining;
    private float _timePerRound = 60f;
    private float _permaTimeBonus;          // Shop upgrade
    public float saverTimeLeft;
    private float _initialSaverTime = 20f;
    
    private int _timeSecondsToAdd;
    private int _saverSecondsToAdd;
    private float _timeAdditionInterval = .15f;
    private float _timeAdditionTimer;

    public int coinCount { get; set; }
    private bool _magneticCoins;            // Shop upgrade
    private int initialCoinPointValue = 50;
    public int coinPointValue;
    
    private int _ballsLocked;
    private int _locksNeededForMultiball = 3;
        
    private int _permaMult;
    private int _ballMultLevel = 1;
    private float _ballMultTimer = 0f;
    private float _ballMultTimerMax = 60f;
    private float _ballMultDecayRate = 1f;
    private int _ballMultMaxedOutBonus = 5000;
    
    private float _nudgeStrength = 1f;      // Shop upgrade
    private float _initialNudgeCooldown = .5f;    // Shop upgrade
    private float _nudgeCooldown;

    public Image nudgeCooldownImage;
    
    public AudioSource audioSource;
    public AudioSource audioSourceNoReverb;
    public AudioSource musicSource;
    public SoundClipCollection soundClips = new SoundClipCollection();
    
    public ScoreLog scoreLog;
    
    void Awake()
    {
        inst = this;
        
        _pauseEvent = new UnityEvent<bool>();
        board = GetComponent<Board>();
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
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Screen.fullScreen = false;
        }
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            musicSource.mute = !musicSource.mute;
        }
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            audioSource.mute = !audioSource.mute;
            audioSourceNoReverb.mute = !audioSourceNoReverb.mute;
        }
        
        if (mode == GameMode.Shop) 
            return;
        
        if (paused)
        {
            return;
        }
        
        #if (UNITY_EDITOR)
        Time.timeScale = Keyboard.current.tKey.isPressed ? .05f : 1f;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            timeRemaining = 0f;
        }
        #endif
        
        if (_timeSecondsToAdd > 0)      // Adding time bonus
        {
            _timeAdditionTimer -= Time.deltaTime;
            if (_timeAdditionTimer <= 0f)
            {
                if (_timeSecondsToAdd > 10 && _timeSecondsToAdd % 10 == 0)
                {
                    timeRemaining += 10f;
                    _timeSecondsToAdd -= 10;
                }
                else
                {
                    timeRemaining += 1f;
                    _timeSecondsToAdd--;    
                }
                
                _timeAdditionTimer = _timeAdditionInterval;
                PlaySound("time_inc", true);    
            }
            
        }
        else if (_timeSecondsToAdd < 0) // Subtracting time penalty
        {
            if (timeRemaining <= 0f)
            {
                // Skip any additional ticks if already at zero
                _timeSecondsToAdd = 0;
            }
            else 
            {
                _timeAdditionTimer -= Time.deltaTime;
                if (_timeAdditionTimer <= 0f)
                {
                    timeRemaining = Mathf.MoveTowards(timeRemaining, 0f, 1f);
                    _timeSecondsToAdd++;
                    _timeAdditionTimer = _timeAdditionInterval;
                    PlaySound("time_dec", true);
                }
            }
        }
        else if (timeRemaining > 0f)    // Normal behavior
        {
            timeRemaining = Mathf.MoveTowards(timeRemaining, 0f, Time.deltaTime);
            if (timeRemaining == 0f)
                PlaySound("time_over");
        }
        
        dotMatrixDisplay.UpdateTimeTex(timeRemaining);
        
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
        
        if (saverTimeLeft > 0f)
        {
            saverTimeLeft -= Time.deltaTime;

            if (saverTimeLeft > 5f)
                saverGraphic.SetMaterial(1);
            if (saverTimeLeft < 5f)
                saverGraphic.SetMaterial((int)saverTimeLeft % 2 == 0 ? 0 : 1);
            else if (saverTimeLeft <= 0f)
                saverGraphic.SetMaterial(0); 
        }
        
        
        // TODO: clean up the flipper button logic
        if (timeRemaining > 0f)
        {
            if (!board.leftFlipperPressed && Keyboard.current.leftShiftKey.isPressed)
            {
                board.leftFlipperPressed = true;
                PlaySound("flipper_up", false);
            }

            if (!board.rightFlipperPressed && Keyboard.current.rightShiftKey.isPressed)
            {
                board.rightFlipperPressed = true;
                PlaySound("flipper_up", false);
            }

            if (board.leftFlipperPressed && !Keyboard.current.leftShiftKey.isPressed)
            {
                board.leftFlipperPressed = false;
                PlaySound("flipper_down", false);
            }

            if (board.rightFlipperPressed && !Keyboard.current.rightShiftKey.isPressed)
            {
                board.rightFlipperPressed = false;
                PlaySound("flipper_down", false);
            }
        }
        else
        {
            if (board.leftFlipperPressed)
            {
                board.leftFlipperPressed = false;
                PlaySound("flipper_down", false);
            }
            if (board.rightFlipperPressed)
            {
                board.rightFlipperPressed = false;
                PlaySound("flipper_down", false);
            }
        }
        
        
        
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Break();
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
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                nudgeVector = Vector2.left;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                nudgeVector = Vector2.right;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
            }
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                nudgeVector = Vector2.up;
                _nudgeCooldown = _initialNudgeCooldown;
                nudgeCooldownImage.gameObject.SetActive(true);
            }

            foreach (GameObject ball in ballDispenser.GetActiveBalls())
            {
                ball.GetComponent<Ball>().AddImpulseForce(nudgeVector * (50f * _nudgeStrength));
            }
        }
        
    }

    void HandleBoardEvent(EventInfo info)
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
                _timeSecondsToAdd += (int)info.Data;
                _timeAdditionTimer = _timeAdditionInterval;
                break;
            case EventSender.EventType.AddBallMult:
                if (_ballMultLevel == 4)
                {
                    _currentScore += _ballMultMaxedOutBonus;
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
                        saverTimeLeft += 15f;
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
                                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                                    DotMatrixDisplay.AnimOrient.Vertical,
                                    .75f, 0, "Ball", TextureWrapMode.Clamp),
                                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                                    DotMatrixDisplay.AnimOrient.Vertical,
                                    .75f, 0, "Locked", TextureWrapMode.Clamp),

                                new (DotMatrixDisplay.AnimType.Hold, 
                                    DotMatrixDisplay.AnimOrient.Horizontal,
                                    1f, 0, ballsLeftTexString, TextureWrapMode.Clamp),
                                new (DotMatrixDisplay.AnimType.Hold, 
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
                saverTimeLeft += (float)info.Data;
                break;
            default:
                Debug.LogError("Unhandled event type:" + info.Type);
                break;
        }
    }

    private void PlaySound(object sound, bool reverb=true)
    {
        var source = reverb ? audioSource : audioSourceNoReverb;
        var clip = sound as AudioClip;
        if (clip != null)
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
        if (mode == GameMode.Play)
        {
            print("Starting multiball");
            multiballEndEvent = new UnityEvent();
            multiballEndEvent.AddListener(EndMultiball);
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
        else
        {
            Debug.LogError("Attempted to start multiball from non-Play mode");
        }
    }

    void EndMultiball()
    {
        SetGameMode(GameMode.Play);
    }

    private void OnDrain(EventInfo info)
    {
        GameObject ball = info.Data as GameObject;
        ballDispenser.DespawnBall(ball);
        PlaySound("latch_clunk", true);
        
        if (timeRemaining <= 0f)
        {
            SetGameMode(GameMode.GameOver);
        }
        else if (saverTimeLeft <= 0f)
        {
            if (ballDispenser.ballsInPlay == 0) 
            {
                _timeSecondsToAdd -= 10;
                if (timeRemaining <= 10f)
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
        else if (mode == GameMode.Countdown && newMode == GameMode.Play)
        {
            ballDispenser.SpawnBall();
            StartNextRound();
        }
        else if (newMode == GameMode.GameOver)
        {
            gameOverLose.gameObject.SetActive(true);
            gameOverLose.scoreText.text = "Final Score: " + _currentScore + "\nPress any key to restart";
        }
        
        
        mode = newMode;
        modeChangeEvent.Invoke();
    }
    
    public UnityEvent<bool> GetPauseEvent()
    {
        return _pauseEvent;
    }
    
    public void StartNewGame()
    {
        MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in monoBehaviours)
        {
            INeedReset inr = mb.GetComponent<INeedReset>();
            if (inr != null)
                inr.ResetForNewGame();
        }
        _currentScore = 0;
        _targetScore = 0;
        _prevTargetScore = 0;
        dotMatrixDisplay.UpdateScoreTex(_currentScore, 0f);
        
        _round = 0;
        saverTimeLeft = _initialSaverTime;
        saverGraphic.SetMaterial(1);
        timeRemaining = 0f;
        _timeSecondsToAdd = 0;
        
        _ballsLocked = 0;
        coinPointValue = initialCoinPointValue;
        coinCount = 0;
        
        mode = GameMode.Countdown;
        countdown.gameObject.SetActive(true);
        countdown.StartCountdown();
    }
    
    void StartNextRound()
    {
        if (_round != 0)
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
        }
        
        _round++;
        if (_round == _totalRounds + 1)
        {
            SetGameMode(GameMode.GameOver);
            gameOverWin.gameObject.SetActive(true);
            gameOverWin.scoreText.text = "Final Score: " + _currentScore + "\nPress any key to restart";
        }

        _timeSecondsToAdd += (int)(_timePerRound + _permaTimeBonus);

        _prevTargetScore = _targetScore;
        _targetScore = (int)(_targetScoreBase * _round * (1f + .25f * (_round - 1))); 
        dotMatrixDisplay.UpdateTargetScoreTex(_targetScore);
        dotMatrixDisplay.UpdateLevelTex(_round, _totalRounds);
        if (scoreLog)
        {
            scoreLog.NextRound();
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
            StartNextRound();
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
    
    public void ApplyItemEffect(Shop.ItemEffect effect, int value)
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
                _permaMult += value;
                timeRemaining += value;
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


