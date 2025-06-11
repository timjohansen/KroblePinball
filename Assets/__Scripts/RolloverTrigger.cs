using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RolloverTrigger : EventSender
{
    public int index;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger, index));
    }
    
}
