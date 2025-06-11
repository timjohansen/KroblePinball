using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class DropTargetMan : EventSender, INeedReset
{
    public DropTargets[] dropTargetObjs;
    int _level;
    bool[] _completed;
    [FormerlySerializedAs("singleDropPoints")] public int initialSingleDropPoints = 250;
    [FormerlySerializedAs("fullDropPoints")] public int initialFullDropPoints = 1000;
    private int _currentSingleDropPoints;
    private int _currentFullDropPoints;
    public float levelMultiplier = .75f;
    
    public SimpleLight[] lightObjs;
    public AudioClip targetDownSound;
    public AudioClip targetsCompleteSound;
    public bool testModePressDToUpgrade;

    void Start()
    {
        if (dropTargetObjs.Length != lightObjs.Length)
            Debug.LogError("Mismatched array lengths in DropTargetMan", gameObject);
            
        for (int i = 0; i < dropTargetObjs.Length; i++)
        {
            dropTargetObjs[i].GetBoardEvent().AddListener(HandleEvent);
        }
        _completed = new bool[dropTargetObjs.Length];
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        SetLevel(0);
        _currentSingleDropPoints = initialSingleDropPoints;
        _currentFullDropPoints = initialFullDropPoints;
    }

    void Update()
    {
        if (testModePressDToUpgrade && Keyboard.current.dKey.wasPressedThisFrame)
        {
            Upgrade(1);
        }
    }
    
    void HandleEvent(EventInfo info)
    {
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
            if (targetDownSound)
                boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, targetDownSound));
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
            if (targetsCompleteSound)
                boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, targetsCompleteSound));

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
