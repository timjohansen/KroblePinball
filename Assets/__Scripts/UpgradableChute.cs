using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UpgradableChute : EventSender, INeedReset
{
    public EventSender progressTrigger;
    public ArrowLights lights;

    public int scoreBase;
    public float scoreMultiplierPerLevel;
    public bool testMode;
    
    private int _level;
    private int _stage;
    private float _resetTimer;
    
    void Start()
    {
        progressTrigger.GetBoardEvent().AddListener(OnProgressEvent);
    }

    public void ResetForNewGame()
    {
        _level = 0;
        _stage = 0;
        _resetTimer = 0;
    }

    void Update()
    {
        if (testMode && Keyboard.current.aKey.wasPressedThisFrame)
        {
            OnProgressEvent(new EventInfo(EventType.Trigger));
        }
        
        if (_stage == 3)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0)
            {
                _stage = 0;
                lights.SetLightStage(_stage);
            }
        }
    }

    void OnProgressEvent(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        if (_stage == 3)
            return;
        
        EventInfo pointInfo = new EventInfo(this, EventType.AddPoints)
        {
            Position3D = lights.transform.position
        };
        _stage++;
        int pointValue;
        if (_stage == 3)
        {
            pointValue = (int)(scoreBase * (1f + _level * scoreMultiplierPerLevel) * 2f);
            Upgrade(1);
        }
        else
        {
            pointValue = (int)(scoreBase * (1f + _level * scoreMultiplierPerLevel));
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "ascending_blips_1"));
        }

        pointInfo.Data = pointValue;
        boardEvent.Invoke(pointInfo);
        lights.SetLightStage(_stage);
    }
    
    public void Upgrade(int numLevels)
    {
        _resetTimer = 2f;
        _level += numLevels;
        lights.SetLevel(_level);
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "level_up_1"));
        
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .2f, 3, "Chutes", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    2f, 0, "Upgraded", TextureWrapMode.Repeat),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }
}
