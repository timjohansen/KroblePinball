using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class ToggleableLetters : EventSender, INeedReset
{
    public bool waitForExternalReset;   // Wait for an external signal before resetting after completion?
    public bool canBeToggledOff;        // Will rolling over a lit letter turn it off? 
    
    public RolloverTrigger[] letterTriggers;
    public Material onMat;
    public Material offMat;
    public AudioClip onSound;
    public AudioClip offSound;
    
    private Renderer _letterRenderer;
    private AudioSource _audioSource;
    protected bool[] _letterIsLit;
    private bool _allLit;
    private float _timeUntilReset;
    private float _blinkTimer;
    private float _blinkRate = .1f;
    
    private void Start()
    {
        _letterRenderer = GetComponent<Renderer>();
        _audioSource = GetComponent<AudioSource>();
        _letterIsLit = new bool[letterTriggers.Length];
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
                for (int i = 0; i < _letterIsLit.Length; i++)
                {
                    _letterIsLit[i] = !_letterIsLit[i];
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
            if (InputMan.inst.leftFlipperPressedThisFrame)
            {
                ShiftLeft();
                UpdateMaterials();
            }

            if (InputMan.inst.rightFlipperPressedThisFrame)
            {
                ShiftRight();
                UpdateMaterials();
            }
        }
    }

    protected virtual void LetterTriggered(EventInfo info)
    {
        if (_allLit)
            return;
        
        if (info.Type != EventType.Trigger || info.Data == null)
            return;
        
        int index = (int)info.Data;
        
        if (canBeToggledOff)
        {
            _letterIsLit[index] = !_letterIsLit[index];
        }
        else
        {
            _letterIsLit[index] = true;
        }

        if (_audioSource)
        {
            if (onSound && _letterIsLit[index])
            {
                _audioSource.PlayOneShot(onSound);
            }
            else if (offSound && !_letterIsLit[index])
            {
                _audioSource.PlayOneShot(offSound);
            }
        }
        
        _allLit = true;
        foreach (bool lit in _letterIsLit)
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
        UpdateMaterials();
    }

    protected void UpdateMaterials()
    {
        List<Material> matList = new();
        foreach (bool lit in _letterIsLit)
        {
            matList.Add(lit ? onMat : offMat);
        }
        _letterRenderer.SetMaterials(matList);
    }

    protected void ShiftRight()
    {
        bool temp = _letterIsLit[^1];    // Store the rightmost value
        for (int i = _letterIsLit.Length - 2; i >= 0; i--)
        {
            _letterIsLit[i + 1] = _letterIsLit[i];
        }
        _letterIsLit[0] = temp;
    }

    protected void ShiftLeft()
    {
        bool temp = _letterIsLit[0];
        for (int i = 0; i  < _letterIsLit.Length - 1; i ++)
        {
            _letterIsLit[i] = _letterIsLit[i + 1];
        }
        _letterIsLit[^1] = temp;
    }

    protected virtual void AllLit()
    { 
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
        if (!waitForExternalReset)
        {
            _timeUntilReset = 2f;
        }
    }

    public override void ExternalReset()
    {
        ResetState();
    }
    
    private void ResetState()
    {
        _allLit = false;
        for (int i = 0; i < _letterIsLit.Length; i++)
        {
            _letterIsLit[i] = false;
        }
        UpdateMaterials();
    }
}
