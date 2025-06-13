using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SequentialTriggers : EventSender, INeedReset
{
    public EventSender[] sequentialTriggers;
    
    public float resetDecayTime = 4f;   // How much time before the sequence automatically resets
    public bool reversable;             // Can the triggers be triggered in reverse order?
    public int scoreValue;              // Should this object give points for completion? 
    public Vector3 scorePosition;       // What position will be passed along in the event info?
    
    private int _nextStep;              // The next trigger expected in the sequence. -1 means no sequence in progress.
    private float _resetTimer;      
    private bool _goingInReverse;       // A reverse sequence is in progress
    
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
            _goingInReverse = false;
            _nextStep = 0;
            Progress();
        }
        else if (reversable && id == sequentialTriggers.Length - 1)
        {
            // If allowed, start a reversed sequence.
            _goingInReverse = true;
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
        EventInfo scoreEventInfo;
        
        if (_goingInReverse)
        {
            _nextStep--;
            if (_nextStep < 0)  // Sequence completed?
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
                boardEvent.Invoke(new EventInfo(this, EventType.Trigger));
                _nextStep = -1;
            }
        }
        _resetTimer = resetDecayTime;
    }
}
