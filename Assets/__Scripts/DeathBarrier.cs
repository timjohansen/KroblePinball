using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBarrier : EventSender
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            GameObject obj = collision.gameObject.GetComponentInParent<Ball>().gameObject;
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger, obj));
        }
    }
}
