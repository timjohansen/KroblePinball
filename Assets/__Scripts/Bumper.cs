using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : EventSender, ICollisionReceiver
{
    public float bounceForce = 1f;    
    public PolygonCollider2D bounceCollider;
    public float variancePercentage = 10f;
    public float minimumIncomingVelocity;
    public AudioClip soundClip;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        ReceiveOnCollisionEnter2D(collision);
    }

    public void ReceiveOnCollisionEnter2D(Collision2D collision)
    {
        if (bounceCollider && collision.otherCollider != bounceCollider)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Ball"))
        {
            if (collision.collider.gameObject.GetComponentInParent<Ball>().speed <= minimumIncomingVelocity)
            {
                return;
            }
            Vector2 direction = -collision.GetContact(0).normal;
            float variance = UnityEngine.Random.value * 2f - 1f;
            variance = variance * bounceForce * variancePercentage * .01f;
            float modifiedForce = bounceForce + variance;
            collision.gameObject.GetComponentInParent<Ball>().AddImpulseForce(direction * modifiedForce);
            EventInfo hitEventInfo = new EventInfo(this, EventType.Trigger, null);
            hitEventInfo.Position2D = collision.GetContact(0).point;
            boardEvent.Invoke(hitEventInfo);
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, soundClip));
        }
    }

    public void ReceiveOnCollisionExit2D(Collision2D collision)
    {
        // Not needed
    }
    public void ReceiveOnCollisionStay2D(Collision2D collision)
    {
        // Not needed
    }
}
