using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KickerTrigger : EventSender
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball"))
        {
            return;
        }
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger, collision.gameObject));
    }
}
