using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradableChute : EventSender, INeedReset
{
    public EventSender trigger;
    public ArrowLights lights;
    
    private int _level;
    private int _stage;
    private float _resetTimer;

    public bool testModePressA;
    
    void Start()
    {
        trigger.GetBoardEvent().AddListener(OnTriggerEvent);
    }

    public void ResetForNewGame()
    {
        _level = 0;
        _stage = 0;
        _resetTimer = 0;
    }

    void Update()
    {
        if (testModePressA && Keyboard.current.aKey.wasPressedThisFrame)
        {
            OnTriggerEvent(null);
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

    void OnTriggerEvent(EventInfo info)
    {
        _stage++;
        EventInfo pointInfo = new EventInfo(this, EventType.AddPoints);
        pointInfo.Position3D = lights.transform.position;
        if (_stage == 3)
        {
            pointInfo.Data = 1000 * (_level + 1);
            boardEvent.Invoke(pointInfo);
            Upgrade(1);
            _stage = 0;
        }
        else
        {
            pointInfo.Data = 500 * (_level + 1);
            boardEvent.Invoke(pointInfo);
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "ascending_blips_1"));
        }
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
                new (DotMatrixDisplay.AnimType.ScrollIn, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .1f, 3, "Chutes", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    1f, 0, "Chutes", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    2f, 0, "Upgraded", TextureWrapMode.Repeat),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }
}
