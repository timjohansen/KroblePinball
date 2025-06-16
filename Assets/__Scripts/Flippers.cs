using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Flippers : EventSender
{
    public static Flippers inst;

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

    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Play || GM.inst.mode == GM.GameMode.Multiball)
        {
            if (GameTime.inst.timeRemaining > 0f)
            {
                if (!leftFlipperPressed && InputMan.inst.leftFlipperPressed)
                {
                    leftFlipperPressed = true;
                    GM.inst.PlaySound("flipper_up", false);
                }

                if (!inst.rightFlipperPressed && InputMan.inst.rightFlipperPressed)
                {
                    rightFlipperPressed = true;
                    GM.inst.PlaySound("flipper_up", false);
                }

                if (leftFlipperPressed && !InputMan.inst.leftFlipperPressed)
                {
                    leftFlipperPressed = false;
                    GM.inst.PlaySound("flipper_down", false);
                }

                if (rightFlipperPressed && !InputMan.inst.rightFlipperPressed)
                {
                    rightFlipperPressed = false;
                    GM.inst.PlaySound("flipper_down", false);
                }
            }
            else
            {
                if (leftFlipperPressed)
                {
                    leftFlipperPressed = false;
                    GM.inst.PlaySound("flipper_down", false);
                }

                if (rightFlipperPressed)
                {
                    rightFlipperPressed = false;
                    GM.inst.PlaySound("flipper_down", false);
                }
            }
        }
    }
}
