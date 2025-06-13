using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionForwarder : MonoBehaviour
{
    // Detects collisions and forwards them to another object along with a reference to itself for ID purposes.
    
    public MonoBehaviour collisionReceiver;     // Unity can't show interfaces in the inspector,
                                                // so this has to be a MonoBehavior, unfortunately.
    private Collider2D _myCollider;
    private ICollisionReceiver _cr;
    private ITriggerReceiver _tr;
    
    void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
        
        if (collisionReceiver is ICollisionReceiver cReceiver)
        {
            _cr = cReceiver;
            return;
        }

        if (collisionReceiver is ITriggerReceiver tReceiver)
        {
            _tr = tReceiver;
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        _cr?.ReceiveOnCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        _cr?.ReceiveOnCollisionExit2D(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        _cr?.ReceiveOnCollisionStay2D(collision);
    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        _tr?.ReceiveOnTriggerEnter2D(collider, _myCollider);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        _tr?.ReceiveOnTriggerExit2D(collider, _myCollider);
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        _tr?.ReceiveOnTriggerStay2D(collider, _myCollider);
    }
}


public interface ICollisionReceiver
{
    public void ReceiveOnCollisionEnter2D(Collision2D collision);    
    public void ReceiveOnCollisionExit2D(Collision2D collision);
    public void ReceiveOnCollisionStay2D(Collision2D collision);
}

public interface ITriggerReceiver
{
    public void ReceiveOnTriggerEnter2D(Collider2D other, Collider2D self);
    public void ReceiveOnTriggerExit2D(Collider2D other, Collider2D self);
    public void ReceiveOnTriggerStay2D(Collider2D other, Collider2D self);
}