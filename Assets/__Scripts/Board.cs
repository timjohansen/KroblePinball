using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Board : EventSender
{
    public static Board inst;

    public int flipperMinAngle;
    public int flipperMaxAngle;
    public float flipperSpeed;
    
    public bool leftFlipperPressed;
    public bool rightFlipperPressed;
    
    public float leftFlipBaseRot;
    public float leftFlipMaxRot;
    public float rightFlipBaseRot;
    public float rightFlipMaxRot;
    
    protected override void Awake()
    {
        base.Awake();
        inst = this;
    }

    void Start()
    {
        leftFlipBaseRot = flipperMinAngle;
        leftFlipMaxRot = flipperMaxAngle;
        rightFlipBaseRot = -flipperMinAngle;
        rightFlipMaxRot = -flipperMaxAngle;
    }
}
