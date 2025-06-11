using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;


public class SimpleBlinkingLight : MonoBehaviour, INeedReset
{
    public int lightMaterialSlot;
    public EventSender onEventSender;
    public EventSender offEventSender;
    public bool startOn;
    private bool _isOn;

    public float blinkSpeed;
    public BlinkType blinkType;
    public Color onColor;
    public Color offColor;
    
    private Renderer _renderer;
    
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        SetState(false);
        
        if (onEventSender)
            onEventSender.GetBoardEvent().AddListener(OnOnEvent);
        if (offEventSender)
            offEventSender.GetBoardEvent().AddListener(OnOffEvent);
    }

    public void ResetForNewGame()
    {
        SetState(startOn);
    }

    
    
    void UpdateMaterial()
    {
        List<Material> mats = new List<Material>();
        _renderer.GetMaterials(mats);
        Material mat = mats[lightMaterialSlot];
        mat.SetInt("_LightOn", (_isOn ? 1 : 0));
        mat.SetFloat("_BlinkSpeed", blinkSpeed);
        mat.SetInt("_BlinkType", (int)blinkType);
        mat.SetColor("_Color_ON", onColor);
        mat.SetColor("_Color_OFF", offColor);
        // _renderer.SetMaterials(mats);
    }
    
    void SetState(bool state)
    {
        _isOn = state;
        UpdateMaterial();
    }

    void OnOnEvent(EventSender.EventInfo info)
    {
        if (info.Type == EventSender.EventType.Trigger)
            SetState(true);
    }
    
    void OnOffEvent(EventSender.EventInfo  info)
    {
        if (info.Type == EventSender.EventType.Trigger)
            SetState(false);
    }

    public enum BlinkType
    {
        Sine, Binary, None
    }
}
