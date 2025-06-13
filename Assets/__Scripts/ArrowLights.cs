using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowLights : MonoBehaviour, INeedReset
{
    // Manages the arrow lights sitting at the base of each upgradable chute. The stage variable indicates which
    // arrow is currently blinking (0 to arrowCount) and arrowCount + 1 causes all arrows to blink rapidly. 
    
    public int arrowCount;
    public ParticleSystem upgradeParticles;
    
    private Renderer _renderer;
    private Color _offColor = new Color(0.2f, 0.2f, 0.2f);
    private int _stage;
    private float _normalBlinkSpeed = 1f;
    
    public bool testMode = false;
    private int _testStage = 0;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        List<Material> mats = new();
        _renderer.GetMaterials(mats);
        foreach (Material mat in mats)
        {
            mat.SetColor("_Color_OFF", _offColor);
        }

        SetLightStage(0);
    }

    public void ResetForNewGame()
    {
        SetLightStage(0);
        SetLevel(0);
    }

    void Update()
    {
        if (testMode && Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            _testStage++;
            _testStage %= arrowCount + 1;
            SetLightStage(_testStage);
        }
    }

    public void SetLightStage(int newStage)
    {
        if (newStage > arrowCount)
        {
            Debug.LogError("Stage out of range", gameObject);
            return;
        }

        _stage = newStage;
        ;
        UpdateMaterials();
    }

    public void SetLevel(int newLevel)
    {
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }

        List<Material> mats = new();
        _renderer.GetMaterials(mats);
        for (int i = 0; i < arrowCount; i++)
        {
            mats[i].SetColor("_Color_ON", GM.inst.levelColors[newLevel]);
        }

        var settings = upgradeParticles.main;
        Color newCol = GM.inst.levelColors[newLevel];
        newCol.a = 1f;
        settings.startColor = new ParticleSystem.MinMaxGradient(newCol);


        if (newLevel > 0)
        {
            upgradeParticles.Play();
        }

        UpdateMaterials();
    }

    void UpdateMaterials()
    {
        List<Material> mats = new();
        _renderer.GetMaterials(mats);

        if (_stage == arrowCount)
        {
            for (int i = 0; i < arrowCount; i++)
            {
                mats[i].SetFloat("_BlinkSpeed", _normalBlinkSpeed * 3f);
                mats[i].SetInt("_LightOn", 1);
                mats[i].SetInt("_BlinkType", 2);
            }
        }
        else
        {
            for (int i = 0; i < arrowCount; i++)
            {
                if (_stage == i)
                {
                    mats[i].SetInt("_LightOn", 1);
                }
                else
                {
                    mats[i].SetInt("_LightOn", 0);
                }
                mats[i].SetFloat("_BlinkSpeed", _normalBlinkSpeed);
                mats[i].SetInt("_BlinkType", 0);
            }
        }

        _renderer.SetMaterials(mats);
    }
}