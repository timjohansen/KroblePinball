using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class EventSender : MonoBehaviour
{
    // A universal base class for any object that communicates with events. The GM automatically subscribes to all
    // EventSenders at the start of the game, but all others are performed manually.
    
    protected UnityEvent<EventInfo> boardEvent;

    protected virtual void Awake()
    {
        boardEvent = new UnityEvent<EventInfo>();
    }

    public UnityEvent<EventInfo> GetBoardEvent()
    {
        return boardEvent;
    }

    public virtual void ExternalReset()
    {
        // Does nothing by default, but can be used by a receiver to reset the sender back to its normal state if
        // being triggered involves multiple states. For example, if lighting all toggleable lights 
        // starts a slot machine, this could be used to keep the letters in a blinking animation until the slot machine
        // finishes spinning before switching the lights back off.
    }
    
    public enum EventType
    {
        Trigger, AddPoints, AddPointsNoMult, AddCoins, AddSeconds, AddBallMult, PlaySound, PlaySoundNoReverb, 
        LockBall, ShowMessage, SpawnBall, AddSaver
    }

    public class EventInfo
    {
        public EventType Type;
        public EventSender Sender;
        public object Data = null;
        public Vector2? Position2D;
        public Vector3? Position3D;

        public EventInfo(EventSender sender, EventType type, object data )
        {
            this.Sender = sender;
            this.Type = type;
            this.Data = data;
        }
        
        public EventInfo(EventSender sender, EventType type)
        {
            this.Sender = sender;
            this.Type = type;
        }
        
        public EventInfo(EventType type)
        {
            this.Type = type;
        }

        public EventInfo()
        {
            this.Type = EventType.Trigger;
        }
    }
}
