using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class JetBumperMan : EventSender, INeedReset
{
    // Handles combined scoring and upgrading for any number of bumper objects. 
    
    public int hitCountTargetBase = 10;         // Hits needed to upgrade to next level
    public float hitTargetLevelMultiplier = 1f; // Rate of increase per level
    public int scoreBase = 100;                 // Score given per bumper hit
    public float scoreLevelMultiplier = .75f;   // Rate of increase per level
    
    public bool testMode;
    
    public RadialMeter meter;
    public EventSender[] bumperSenders;
    public ParticleSystem upgradeParticles;
    
    private int _level;
    private int _hitCount;
    private int _hitCountTarget;
    
    

    protected void Start()
    {
        foreach (EventSender sender in bumperSenders)
        {
            sender.GetBoardEvent().AddListener(OnBumperHit);
        }
        ResetForNewGame();
    }

    void Update()
    {
#if (UNITY_EDITOR)
        if (testMode && Keyboard.current.jKey.wasPressedThisFrame)
        {
            Upgrade(1);
        }
#endif
    }
    
    public void ResetForNewGame()
    {
        SetLevel(0);
        _hitCount = 0;
        _hitCountTarget = hitCountTargetBase;
    }

    void OnBumperHit(EventInfo info)
    {
        _hitCount++;
        EventInfo pointEventInfo = new EventInfo(this, EventType.AddPoints, (int)(scoreBase * (_level + 1) * scoreLevelMultiplier));
        if (info.Position2D.HasValue)
        {
            // Forwards the location of the impact if present, used for effects.
            pointEventInfo.Position2D = info.Position2D.Value;
        }
        boardEvent.Invoke(pointEventInfo);

        if (_hitCount >= _hitCountTarget)
        {
            Upgrade(1);
        }
        meter.SetPercentage((float)_hitCount / _hitCountTarget);
    }

    void SetLevel(int newLevel)
    {
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }
        _level = newLevel;
        _hitCount = 0;
        _hitCountTarget = (int)(hitCountTargetBase * (_level + 1) * hitTargetLevelMultiplier);
        
        meter.SetPercentage(0f);
        
        // Set meter colors
        Color backgroundColor;
        if (_level > 0)
        {
            backgroundColor = GM.inst.levelColors[_level - 1];
            backgroundColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1f);
        }
        else
        {
            backgroundColor = new Color(1f, 1f, 1f, 0f);    
        }
                
        Color progressColor = GM.inst.levelColors[_level];    
        progressColor = new Color(progressColor.r, progressColor.g, progressColor.b, 1f);
        
        meter.SetColors(progressColor, backgroundColor);
        
        // Change material color and trigger upgrade particles
        ObjectLink objLink = GetComponent<ObjectLink>();
        if (objLink && objLink.obj3D)
        {
            objLink.obj3D.GetComponent<Renderer>().material.SetColor("_Color_ON", GM.inst.levelColors[_level]);
            if (upgradeParticles && _level > 0)
            {
                var particleSettings = upgradeParticles.main;
                Color newCol = GM.inst.levelColors[_level];
                newCol.a = 1f;
                particleSettings.startColor = new ParticleSystem.MinMaxGradient(newCol);   
                upgradeParticles.Play();
            }
        }
    }
    
    public void Upgrade(int numLevels)
    {
        SetLevel(_level + numLevels);
        
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "level_up_1"));
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollIn, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .1f, 3, "Bumpers", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    1f, 0, "Bumpers", TextureWrapMode.Repeat),
                new(DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    2f, 0, "Upgraded", TextureWrapMode.Repeat),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }
}
