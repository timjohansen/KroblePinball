using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleLight : MonoBehaviour, INeedReset
{
    // public Material offMat;
    // public Material onMat;
    private bool _lightOn;
    private bool _stateChanged;
    public Color initialColor = new Color(1, 1, 1, 1);
    private Color _color;
    public BlinkType initialBlinkType;
    private BlinkType _blinkType;

    void Start()
    {
        _color = initialColor;
        _blinkType = initialBlinkType;
        SetBlinkType(initialBlinkType);
        UpdateMaterial();
    }

    public void ResetForNewGame()
    {
        LightOff();
        
    }

    public void LightOn()
    {
        _lightOn = true;
        _stateChanged = true;
    }

    public void LightOff()
    {
        _lightOn = false;
        _stateChanged = true;
    }

    public void SetColor(Color color)
    {
        _color = color;
        UpdateMaterial();
    }

    public void SetBlinkType(BlinkType type)
    {
        gameObject.GetComponent<MeshRenderer>().material.SetInt("_BlinkType", (int)type);
    }
    
    void UpdateMaterial()
    {
        Material mat = gameObject.GetComponent<MeshRenderer>().material;
        if (_lightOn)
            mat.SetInt("_LightOn", 1);
        else
            mat.SetInt("_LightOn", 0);
        mat.SetColor("_Color", _color);
    }

    void LateUpdate()
    {
        if (_stateChanged)
            UpdateMaterial();
    }

    public enum BlinkType
    {
        Sine, Binary, None
    }
}
