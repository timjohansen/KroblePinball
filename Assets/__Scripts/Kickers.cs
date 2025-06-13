using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class Kickers : EventSender, INeedReset
{
    // Handles launching balls out of the outlanes. 
    
    public ObjectLink leftCap;
    public ObjectLink rightCap;
    public EventSender capOpenTrigger;
    public EventSender leftKickTrigger;
    public EventSender rightKickTrigger;
    
    // Time before the caps close after kicker fires. Collider effectors allow the ball
    // to pass through regardless, so this is just for visual effect.
    public float closeTimerLength;
    private float _leftCloseTimer;
    private float _rightCloseTimer;
    
    public float kickStrength = 1000f;
    public bool testMode;

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
        SetCapState("left", true);
        SetCapState("right", true);
    }

    private void Update()
    {
#if (UNITY_EDITOR)
        if (testMode && Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnOpenCapsEvent(new EventInfo(this, EventType.Trigger));
        }
#endif

        if (_leftCloseTimer > 0f)
        {
            _leftCloseTimer = Mathf.MoveTowards(_leftCloseTimer, 0f, Time.deltaTime);
            if (_leftCloseTimer <= 0f)
            {
                SetCapState("left", true);
            }
        }
        if (_rightCloseTimer > 0f)
        {
            _rightCloseTimer = Mathf.MoveTowards(_rightCloseTimer, 0f, Time.deltaTime);
            if (_rightCloseTimer <= 0f)
            {
                SetCapState("right", true);
            }
        }
    }

    private void SetCapState(string side, bool state)
    {
        if (side == "left")
        {
            leftCap.obj2D.SetActive(state);
            leftCap.obj3D.SetActive(state);
        }
        else if (side == "right")
        {
            rightCap.obj2D.SetActive(state);
            rightCap.obj3D.SetActive(state);
        }
    }

    private void BallEnteredLeft(EventInfo info){
        if (info.Type != EventType.Trigger || info.Data == null)
            return;
        
        GameObject ballObj = (GameObject)info.Data;
        if (!ballObj)
            return;

        Ball ball = ballObj.GetComponentInParent<Ball>();
        if (!ball)
            return;
        
        ball.AddImpulseForce(new Vector2(0f, kickStrength));
        _leftCloseTimer = closeTimerLength;
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "plunger"));
    }

    private void BallEnteredRight(EventInfo info){
        if (info.Type != EventType.Trigger || info.Data == null)
            return;
        
        GameObject ballObj = (GameObject)info.Data;
        if (!ballObj)
            return;

        Ball ball = ballObj.GetComponentInParent<Ball>();
        if (!ball)
            return;
        
        ball.AddImpulseForce(new Vector2(0f, kickStrength));
        _rightCloseTimer = closeTimerLength;
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "plunger"));
    }

    private void OnOpenCapsEvent(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        SetCapState("left", false);
        SetCapState("right", false);
        
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
