using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class JetBumperMan : EventSender, INeedReset
{
    public int level;
    public int hitCount;
    public int hitCountTarget;
    public int hitTargetBase = 10;
    public float hitTargetLevelMultiplier = 1f;
    public int scoreBase = 100;
    public float scoreLevelMultiplier = .75f;

    public GameObject obj3D;
    public RadialMeter meter;
    public EventSender[] bumperSenders;
    
    public ParticleSystem upgradeParticles;
    public bool testModePressJ;

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
        if (testModePressJ && Keyboard.current.jKey.wasPressedThisFrame)
        {
            Upgrade(1);
        }
    }
    
    public void ResetForNewGame()
    {
        SetLevel(0);
        hitCount = 0;
        hitCountTarget = hitTargetBase;

    }

    void OnBumperHit(EventInfo info)
    {
        hitCount++;
        EventInfo pointEventInfo = new EventInfo(this, EventType.AddPoints, (int)(scoreBase * (level + 1) * scoreLevelMultiplier));
        if (info.Position2D.HasValue)
        {
            pointEventInfo.Position2D = info.Position2D.Value;
        }
        boardEvent.Invoke(pointEventInfo);

        if (hitCount >= hitCountTarget)
        {
            Upgrade(1);
        }
        meter.SetPercentage((float)hitCount / hitCountTarget);
    }

    void SetLevel(int newLevel)
    {
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }
        level = newLevel;
        hitCount = 0;
        hitCountTarget = (int)(hitTargetBase * (level + 1) * hitTargetLevelMultiplier);

        meter.SetPercentage(0f);
        
        // Gets the colors for the current and next level while ensuring the indices stays within array bounds
        Color backgroundColor;
        if (level > 0)
        {
            backgroundColor = GM.inst.levelColors[level];
            backgroundColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1f);
        }
        else
        {
            backgroundColor = new Color(1f, 1f, 1f, 0f);    
        }
                
        Color progressColor = GM.inst.levelColors[Math.Clamp(level + 1, 0, GM.inst.levelColors.Length - 1)];    
        progressColor = new Color(progressColor.r, progressColor.g, progressColor.b, 1f);
        
        meter.SetColors(progressColor, backgroundColor);
        
        if (obj3D && level < GM.inst.levelColors.Length)
        {
            obj3D.GetComponent<Renderer>().material.SetColor("_Color_ON", GM.inst.levelColors[newLevel]);
            var settings = upgradeParticles.main;
            Color newCol = GM.inst.levelColors[newLevel];
            newCol.a = 1f;
            settings.startColor = new ParticleSystem.MinMaxGradient(newCol);
        }
        
        if (level > 0 && upgradeParticles)
        {
            upgradeParticles.Play();
        }
    }
    
    public void Upgrade(int numLevels)
    {
        SetLevel(level + numLevels);
        
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
    
    public override void ResetState()
    {
        // throw new System.NotImplementedException();
    }
}
