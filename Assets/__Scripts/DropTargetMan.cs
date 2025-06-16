using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class DropTargetMan : EventSender, INeedReset
{
    // Manages the combined scoring and upgrading for any number of DropTargets objects.
    
    public DropTargets[] dropTargetObjs;
    public ShaderLight[] lightObjs;
    
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
        
        for (int i = 0; i < dropTargetObjs.Length; i++)
        {
            if (dropTargetObjs[i] == info.Sender)
            {
                print("Sender:" + info.Sender.name);
                _completed[i] = true;
                lightObjs[i].SetState(true);
                break;
            }
        }
        
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
            lightObjs[i].SetState(false);
            lightObjs[i].SetOnColor(GM.inst.levelColors[_level]);
            
            dropTargetObjs[i].SetLevel(newLevel);
        }
    }
}
