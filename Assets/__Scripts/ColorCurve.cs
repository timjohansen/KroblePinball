using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ColorCurve : EventSender, INeedReset
{

    private int _level;
    public int currentValue;
    int _maxValue;
    private Material _mat;
    private static readonly int Percentage = Shader.PropertyToID("_Percentage");
    public EventSender incrementTrigger;
    
    public AudioClip[] progressSounds;
    
    private void Start()
    {
        incrementTrigger.GetBoardEvent().AddListener(OnIncrement);
        _mat = GetComponent<Renderer>().material;
        _mat.SetFloat(Percentage, 0f);
    }

    public void ResetForNewGame()
    {
        currentValue = 0;
        _level = 1;
        _maxValue = _level * 8;
        UpdateMaterial();
    }

    private void OnIncrement(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        if (currentValue % _level == 0)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, progressSounds[currentValue / _level]));    
        }
        
        currentValue++;
        
        if (currentValue == _maxValue)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
            _level++;
            _maxValue = _level  * 8;
            currentValue = 0;
        }
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        _mat.SetFloat(Percentage, (float)currentValue / _maxValue);
    }
}
