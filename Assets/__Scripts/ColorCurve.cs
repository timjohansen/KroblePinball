using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ColorCurve : EventSender, INeedReset
{
    
    public int currentValue;
    public int maxValue;
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
        UpdateMaterial();
    }

    private void OnIncrement(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        if (currentValue % 2 == 1)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, progressSounds[currentValue / 2]));    
        }
        
        currentValue++;
        
        if (currentValue == maxValue)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
            currentValue = 0;
        }
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        _mat.SetFloat(Percentage, (float)currentValue / maxValue);
    }
}
