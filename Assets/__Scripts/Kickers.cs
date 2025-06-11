using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class Kickers : EventSender, INeedReset
{
    public GameObject leftCap3D;
    public GameObject rightCap3D;

    public GameObject leftCap2D;
    public GameObject rightCap2D;

    public float closeTimerLength;
    private float _leftCloseTimer;
    private float _rightCloseTimer;

    [FormerlySerializedAs("capOpenEvent")] public EventSender capOpenTrigger;
    [FormerlySerializedAs("leftKickEvent")] public EventSender leftKickTrigger;
    [FormerlySerializedAs("rightKickEvent")] public EventSender rightKickTrigger;

    public float kickStrength = 600f;
    public bool testModePressKToTrigger;

    private void Start()
    {
        capOpenTrigger.GetBoardEvent().AddListener(OnOpenCapsEvent);
        leftKickTrigger.GetBoardEvent().AddListener(BallEnteredLeft);
        rightKickTrigger.GetBoardEvent().AddListener(BallEnteredRight);
    }

    public void ResetForNewGame()
    {
        _leftCloseTimer = 0f;
        _rightCloseTimer = 0f;
        SetCapClosed("left", true);
        SetCapClosed("right", true);
    }

    private void Update()
    {
        if (testModePressKToTrigger && Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnOpenCapsEvent(new EventInfo(this, EventType.Trigger));
        }

        if (_leftCloseTimer > 0f)
        {
            _leftCloseTimer -= Time.deltaTime;
            if (_leftCloseTimer < 0f)
            {
                SetCapClosed("left", true);
            }
        }
        if (_rightCloseTimer > 0f)
        {
            _rightCloseTimer -= Time.deltaTime;
            if (_rightCloseTimer < 0f)
            {
                SetCapClosed("right", true);
            }
        }
    }

    private void SetCapClosed(string side, bool state)
    {
        if (side == "left")
        {
            leftCap2D.SetActive(state);
            leftCap3D.SetActive(state);
        }
        else if (side == "right")
        {
            rightCap2D.SetActive(state);
            rightCap3D.SetActive(state);
        }
    }

    private void BallEnteredLeft(EventInfo info){
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        GameObject ball = (GameObject)info.Data;
        ball.GetComponentInParent<Ball>().AddImpulseForce(new Vector2(0f, kickStrength));
        _leftCloseTimer = closeTimerLength;
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "plunger"));
    }

    private void BallEnteredRight(EventInfo info){
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        GameObject ball = (GameObject)info.Data;
        ball.GetComponentInParent<Ball>().AddImpulseForce(new Vector2(0f, kickStrength));
        _rightCloseTimer = closeTimerLength;
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "plunger"));
    }

    private void OnOpenCapsEvent(EventInfo info)
    {
        if (info.Type != EventSender.EventType.Trigger)
        {
            return;
        }
        SetCapClosed("left", false);
        SetCapClosed("right", false);
        
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "success_minor"));
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    1f, 0, "Kickback", TextureWrapMode.Clamp),
                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    1f, 0, "Ready", TextureWrapMode.Clamp),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }
}
