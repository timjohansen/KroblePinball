using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingDoor : MonoBehaviour, INeedReset
{
    public EventSender openSender;
    public EventSender closeSender;
    public GameObject door;
    public float animationDuration = .5f;
    public Vector3 openRotation;
    public bool testMode;
    
    private bool _isOpen;
    private float _animationTimer;
    void Start()
    {
        if (openSender)
            openSender.GetBoardEvent().AddListener(OpenDoor);
        if (closeSender)
            closeSender.GetBoardEvent().AddListener(CloseDoor);
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        CloseDoor(new EventSender.EventInfo());
    }

    void Update()
    {
        if (_animationTimer > 0f)
        {
            float lerpValue;
            if (_isOpen)
                lerpValue = Mathf.InverseLerp(animationDuration, 0f, _animationTimer);
            else
            {
                lerpValue = Mathf.InverseLerp(0f, animationDuration, _animationTimer);
            }
            Quaternion newRotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, openRotation, lerpValue));
            door.transform.localRotation = newRotation;
            _animationTimer -= Time.deltaTime;
        }

        if (testMode)
        {
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                if (_isOpen)
                {
                    CloseDoor(new EventSender.EventInfo());
                }
                else
                {
                    OpenDoor(new EventSender.EventInfo());
                }
            }
        }
    }

    void OpenDoor(EventSender.EventInfo info)
    {
        if (info.Type != EventSender.EventType.Trigger)
            return;
        _isOpen = true;
        _animationTimer = animationDuration;
    }

    void CloseDoor(EventSender.EventInfo info)
    {
        if (info.Type != EventSender.EventType.Trigger)
            return;
        _isOpen = false;
        _animationTimer = animationDuration;
    }
}
