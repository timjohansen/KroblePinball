using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public class ToggleableLetters : EventSender, INeedReset
{
    public bool waitForExternalReset;
    public bool canBeToggledOff;
    
    public MeshRenderer letterRenderer;
    public Material on_mat;
    public Material off_mat;
    public AudioClip onSound;
    public AudioClip offSound;
    
    protected bool[] letterIsLit;
    public RolloverTrigger[] letterTriggers;

    private bool _allLit;
    private float _timeUntilReset;
    private float _blinkTimer;
    private float _blinkRate = .1f;
    
    private void Start()
    {
        letterIsLit = new bool[letterTriggers.Length];
        for (int i = 0; i < letterTriggers.Length; i++)
        {
            letterTriggers[i].GetBoardEvent().AddListener(LetterTriggered);
        }
        UpdateMaterials();
    }

    public void ResetForNewGame()
    {
        ResetState();
    }

    protected void Update()
    {
        if (_allLit)
        {            
            _blinkTimer += Time.deltaTime;
            if (_blinkTimer > _blinkRate)
            {
                for (int i = 0; i < letterIsLit.Length; i++)
                {
                    letterIsLit[i] = !letterIsLit[i];
                }
                _blinkTimer %= _blinkRate;
                UpdateMaterials();
            }
            if (!waitForExternalReset)
            {
                _timeUntilReset -= Time.deltaTime;
                if (_timeUntilReset <= 0f)
                {
                    ResetState();
                }
            }
        }
        else
        {
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                ShiftLeft();
                UpdateMaterials();
            }

            if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
            {
                ShiftRight();
                UpdateMaterials();
            }
        }
    }

    protected virtual void LetterTriggered(EventInfo info)
    {
        if (_allLit)
        {
            return;
        }
        int index = (int)info.Data;
        if (canBeToggledOff)
        {
            letterIsLit[index] = !letterIsLit[index];
        }
        else
        {
            letterIsLit[index] = true;
        }

        if (onSound && letterIsLit[index])
        {
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, onSound));
        }
        else if (offSound && !letterIsLit[index])
        {
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, offSound));
        }
        
        UpdateMaterials();
        _allLit = true;

        foreach (bool lit in letterIsLit)
        {
            if (!lit)
            {
                _allLit = false;
            }
        }

        if (_allLit)
        {
            AllLit();
        }
    }

    protected void UpdateMaterials()
    {
        List<Material> matList = new();
        foreach (bool letter in letterIsLit)
        {
            if (letter)
            {
                matList.Add(on_mat);
            }
            else
            {
                matList.Add(off_mat);
            }
        }

        letterRenderer.SetMaterials(matList);
    }

    protected void ShiftRight()
    {
        bool temp = letterIsLit[^1];    // Store the rightmost value
        for (int i = letterIsLit.Length - 2; i >= 0; i--)
        {
            letterIsLit[i + 1] = letterIsLit[i];
        }
        letterIsLit[0] = temp;
    }

    protected void ShiftLeft()
    {
        bool temp = letterIsLit[0];
        for (int i = 0; i  < letterIsLit.Length - 1; i ++)
        {
            letterIsLit[i] = letterIsLit[i + 1];
        }
        letterIsLit[^1] = temp;
    }

    protected virtual void AllLit()
    { 
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
        if (!waitForExternalReset)
        {
            _timeUntilReset = 2f;
        }
    }

    public override void ResetState()
    {
        _allLit = false;
        for (int i = 0; i < letterIsLit.Length; i++)
        {
            letterIsLit[i] = false;
        }
        UpdateMaterials();
    }

}
