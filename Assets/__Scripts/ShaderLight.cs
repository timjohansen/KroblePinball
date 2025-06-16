using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public class ShaderLight : MonoBehaviour, INeedReset
{
    // This class is for managing the state of a ColorLight shader.
    
    public int lightMaterialSlot;           // Which material slot has the ColorLight shader
    public EventSender onEventSender;       // What objects will send Trigger events to turn off/on the light (optional)
    public EventSender offEventSender;      
    public bool startOn;
    private bool _isOn;

    public float blinkSpeed = 1;            // Blink rate for Sine and Binary types
    public BlinkType blinkType;
    public Color onColor;
    public Color offColor;
    
    private Renderer _renderer;
    private List<Material> _materials = new List<Material>();
    
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.GetMaterials(_materials);
        
        if (onEventSender)
            onEventSender.GetBoardEvent().AddListener(OnOnEvent);
        if (offEventSender)
            offEventSender.GetBoardEvent().AddListener(OnOffEvent);

        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        SetState(startOn);
    }
    
    void UpdateMaterial()
    {
        Material mat = _materials[lightMaterialSlot];
        mat.SetInt("_LightOn", (_isOn ? 1 : 0));
        mat.SetFloat("_BlinkSpeed", blinkSpeed);
        mat.SetInt("_BlinkType", (int)blinkType);
        mat.SetColor("_Color_ON", onColor);
        mat.SetColor("_Color_OFF", offColor);
    }
    
    public void SetState(bool state)
    {
        _isOn = state;
        UpdateMaterial();
    }

    public void SetOnColor(Color color)
    {
        onColor = color;
        UpdateMaterial();
    }

    public void SetOffColor(Color color)
    {
        offColor = color;
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
