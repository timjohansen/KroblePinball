using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallLocker : RolloverTrigger
{
    // Trigger for a "ball lock", which despawns the ball and progresses the counter toward multiball.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            boardEvent.Invoke(new EventInfo(this, EventType.LockBall, collision.gameObject.GetComponentInParent<Ball>()));
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger));
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "latch_clunk_2"));
        }
    }
}
