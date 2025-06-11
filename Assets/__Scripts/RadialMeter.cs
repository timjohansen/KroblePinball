using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMeter : MonoBehaviour
{
    private Material _progressMat;    

    void Awake()
    {
        Material[] matArray = GetComponent<MeshRenderer>().materials;
        _progressMat = matArray[0];
    }
    
    public void SetColors(Color progressColor, Color backgroundColor)
    {
        _progressMat.SetColor("_FillColor", progressColor);
        _progressMat.SetColor("_BackgroundColor", backgroundColor);
    }
    public void SetPercentage(float _percentComplete)
    {
        _percentComplete = Math.Clamp(_percentComplete, 0f, 1f);
        _progressMat.SetFloat("_Percentage", _percentComplete);
    }
}
