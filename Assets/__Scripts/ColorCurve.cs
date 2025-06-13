using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ColorCurve : EventSender, INeedReset
{
    public EventSender incrementTrigger;
    public AudioClip[] progressSounds;
    
    private int _level;
    private int _currentValue;
    private int _maxValue;
    private bool _soundReady;
    private Material _mat;
    private static readonly int Percentage = Shader.PropertyToID("_Percentage");
    
    private void Start()
    {
        incrementTrigger.GetBoardEvent().AddListener(OnIncrement);
        _mat = GetComponent<Renderer>().material;
        
        // Make sure that each array slot has a sound clip assigned
        _soundReady = true;
        for (int i = 0; i < progressSounds.Length; i++)
        {
            if (!progressSounds[i])
            {
                _soundReady = false;
            }
        }
    }

    public void ResetForNewGame()
    {
        _currentValue = 0;
        _level = 1;
        _maxValue = _level * progressSounds.Length;
        
        _mat.SetFloat(Percentage, 0f);
        
        UpdateMaterial();
    }

    private void OnIncrement(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        
        if (_soundReady && _currentValue % _level == 0)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, progressSounds[_currentValue / _level]));    
        }
        
        _currentValue++;
        
        if (_currentValue == _maxValue)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
            _level++;
            _currentValue = 0;
            _maxValue = _level * progressSounds.Length;
        }
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        _mat.SetFloat(Percentage, (float)_currentValue / _maxValue);
    }
}
