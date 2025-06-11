using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SequentialTriggers : EventSender, INeedReset
{
    public EventSender[] sequentialTriggers;
    
    public float resetDecayTime = 4f;
    public bool reversable;
    
    private int _nextStep;
    private float _resetTimer;
    private bool _reverseDir;

    public int scoreValue;
    public Vector3 scorePosition;
    

    void Start()
    {
        foreach (EventSender sender in sequentialTriggers)
        {
            sender.GetBoardEvent().AddListener(HandleTrigger);
        }

        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        _nextStep = -1;
    }

    void Update()
    {
        if (_nextStep > -1)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer < 0f)
            {
                _nextStep = -1;
            }
        }
    }

    void HandleTrigger(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        int id = (int)info.Data;
        
        if (id == _nextStep)
        {
            // Ball has hit the next trigger in the sequence
            Progress();
        }
        else if (id == 0)
        {
            // Ball has hit the first trigger in the sequence after previously failing to complete it.
            _reverseDir = false;
            _nextStep = 0;
            Progress();
        }
        else if (reversable && id == sequentialTriggers.Length - 1)
        {
            // If allowed, start a reversed sequence.
            _reverseDir = true;
            _nextStep = sequentialTriggers.Length - 1;
            Progress();
        }
        else
        {
            // Ball his hit a trigger out of sequence.
            print("Out of order");
            _nextStep = -1;
        }
    }

    void Progress()
    {
        EventInfo scoreEventInfo = new EventInfo();
        
        if (_reverseDir)
        {
            _nextStep--;
            if (_nextStep < 0)
            {
                if (scoreValue > 0)
                {
                    scoreEventInfo = new EventInfo(this, EventType.AddPoints, scoreValue);
                    if (scorePosition != Vector3.zero)
                    {
                        scoreEventInfo.Position3D = scorePosition;
                    }
                    boardEvent.Invoke(scoreEventInfo);
                }
                boardEvent.Invoke(scoreEventInfo);
                boardEvent.Invoke(new EventInfo(EventType.Trigger));
                _nextStep = -1;
            }
        }
        else
        {
            _nextStep++;
            if (_nextStep > sequentialTriggers.Length - 1)
            {
                if (scoreValue > 0)
                {
                    scoreEventInfo = new EventInfo(this, EventType.AddPoints, scoreValue);
                    if (scorePosition != Vector3.zero)
                    {
                        scoreEventInfo.Position3D = scorePosition;
                    }
                    boardEvent.Invoke(scoreEventInfo);
                }
                boardEvent.Invoke(scoreEventInfo);
                boardEvent.Invoke(new EventInfo(EventType.Trigger));
                _nextStep = -1;
            }
        }
        _resetTimer = resetDecayTime;
    }
}
