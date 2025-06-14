using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class DropTargetMan : EventSender, INeedReset
{
    // Manages the combined scoring and upgrading for any number of DropTargets objects.
    
    public DropTargets[] dropTargetObjs;
    public SimpleLight[] lightObjs;
    public AudioClip targetDownSound;
    public AudioClip targetsCompleteSound;
    
    public int initialSingleDropPoints = 250;
    public int initialFullDropPoints = 1000;
    public float levelMultiplier = .75f;
    private int _currentSingleDropPoints;
    private int _currentFullDropPoints;
    
    private bool _initialized;
    private int _level;
    private bool[] _completed;
    
    public bool testMode;

    void Start()
    {
        if (dropTargetObjs.Length != lightObjs.Length)
        {
            Debug.LogError("Mismatched array lengths in DropTargetMan", gameObject);
            return;
        }
            
        for (int i = 0; i < dropTargetObjs.Length; i++)
        {
            dropTargetObjs[i].GetBoardEvent().AddListener(HandleEvent);
        }
        _completed = new bool[dropTargetObjs.Length];
        _initialized = true;
        
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        if (!_initialized)
            return;
        
        SetLevel(0);
        _currentSingleDropPoints = initialSingleDropPoints;
        _currentFullDropPoints = initialFullDropPoints;
    }

    void Update()
    {
        if (!_initialized)
            return;
        
#if (UNITY_EDITOR)
        if (testMode && Keyboard.current.dKey.wasPressedThisFrame)
        {
            Upgrade(1);
        }
#endif
    }
    
    void HandleEvent(EventInfo info)
    {
        if (!_initialized)
            return;
        
        EventInfo hitInfo;
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        if (info.Data == null)    // Single hit
        {
            hitInfo = new EventInfo(this, EventType.AddPoints, (int)(initialSingleDropPoints * (_level + 1) * levelMultiplier));
            if (info.Position2D.HasValue)
            {
                hitInfo.Position2D = info.Position2D.Value;
            }
            boardEvent.Invoke(hitInfo);
        }
        else
        {
            string objStr = (string)info.Data;
            for (int i = 0; i < dropTargetObjs.Length; i++)
            {
                if (dropTargetObjs[i].name == objStr)
                {
                    _completed[i] = true;
                    lightObjs[i].LightOn();
                    break;
                }
            }

            hitInfo = new EventInfo(this, EventType.AddPoints, (int)(initialFullDropPoints * (_level + 1) * levelMultiplier));
            if (info.Position2D.HasValue)
            {
                hitInfo.Position2D = info.Position2D.Value;
            }
            boardEvent.Invoke(hitInfo);

            bool allComplete = true;
            for (int i = 0; i < dropTargetObjs.Length; i++)
            {
                allComplete = allComplete && _completed[i];
            }

            if (allComplete)
            {
                Upgrade(1);
            }
        }
    }

    public void Upgrade(int levels)
    {
        if (!_initialized)
            return;
        
        SetLevel(_level + levels);
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "level_up_1"));
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .5f, 3, "Drops", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    2f, 0, "Upgraded", TextureWrapMode.Repeat),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }

    void SetLevel(int newLevel)
    {
        if (!_initialized)
            return;
        
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }
        _level = newLevel;
        for (int i = 0; i < dropTargetObjs.Length; i++)
        {
            _completed[i] = false;
            lightObjs[i].LightOff();
            int levelIndex = _level % GM.inst.levelColors.Length;
            lightObjs[i].SetColor(GM.inst.levelColors[levelIndex]);
            dropTargetObjs[i].SetLevel(newLevel);
        }
    }
}
