using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class FlipperPre : MonoBehaviour
{
    public bool rightSide;
    public float rotateValue;
    public float angle;
    public float prevAngle;
    public bool swingingUp;
    bool flipperPressedPrevFrame;
    
    List<GameObject> ballsInContact;
    List<GameObject> parentedBalls;
    ObjectLink objLink;
    
    void Awake()
    {
        objLink = GetComponent<ObjectLink>();
        ballsInContact = new();
        parentedBalls = new();

    }

    void FixedUpdate()
    {
        bool flipperPressed;
        float flipperBaseRot;
        float flipperMaxRot;

        if (rightSide)
        {
            flipperPressed = Board.inst.rightFlipperPressed;

            flipperBaseRot = Board.inst.rightFlipBaseRot;
            flipperMaxRot = Board.inst.rightFlipMaxRot;
        }
        else
        {
            flipperPressed = Board.inst.leftFlipperPressed;
            flipperBaseRot = Board.inst.leftFlipBaseRot;
            flipperMaxRot = Board.inst.leftFlipMaxRot;
        }
        
        float prevRotateValue = rotateValue;
        prevAngle = Mathf.Lerp(flipperBaseRot, flipperMaxRot, rotateValue);
        angle = prevAngle;
        
        if (flipperPressed)
        {
            if (rotateValue < 1f)
            {
                rotateValue = Mathf.MoveTowards(rotateValue, 1f, Board.inst.flipperSpeed * Time.fixedDeltaTime);
                angle = Mathf.Lerp(flipperBaseRot, flipperMaxRot, rotateValue);
            }            
        }
        else
        {
            if (rotateValue > 0f)
            {
                rotateValue = Mathf.MoveTowards(rotateValue, 0f, Board.inst.flipperSpeed * Time.fixedDeltaTime);
                angle = Mathf.Lerp(flipperBaseRot, flipperMaxRot, rotateValue);
            }
        }

        swingingUp = prevRotateValue < rotateValue;
    }

    



}
