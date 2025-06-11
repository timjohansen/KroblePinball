using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class EventSender : MonoBehaviour
{
    protected UnityEvent<EventInfo> boardEvent;

    protected virtual void Awake()
    {
        boardEvent = new UnityEvent<EventInfo>();
    }

    public UnityEvent<EventInfo> GetBoardEvent()
    {
        return boardEvent;
    }

    public virtual void ResetState()
    {
        // Does nothing by default
        // TODO: decide if this is worth keeping
    }

    public virtual void SetFirstMaterial(Material newMat)
    {
        ObjectLink objLink = GetComponent<ObjectLink>();
        if (objLink)
        {
            MeshRenderer mr = objLink.obj3D.GetComponentInChildren<MeshRenderer>();
            if (mr != null)
            {
                mr.material = newMat;
                return;
            }
            SkinnedMeshRenderer smr = objLink.obj3D.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr)
            {
                smr.material = newMat;
                return;
            }
        }
        Debug.LogError("ObjectLink not found when attempting to set material", gameObject);
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
