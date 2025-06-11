using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionForwarder : MonoBehaviour
{
    public MonoBehaviour receiverObject;
    private ICollisionReceiver _cr;
    private ITriggerReceiver _tr;
    private Collider2D _myCollider;
    
    void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
        try
        {
            _cr = (ICollisionReceiver)receiverObject;
        }
        catch
        {
            _cr = null;
        }
        
        try
        {
            _tr = (ITriggerReceiver)receiverObject;
        }
        catch
        {
            _tr = null;
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
        if (_tr == null)
        {
            return;
        }
        _tr.ReceiveOnTriggerEnter2D(collider, _myCollider);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (_tr == null)
        {
            return;
        }
        _tr.ReceiveOnTriggerExit2D(collider, _myCollider);
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (_tr == null)
        {
            return;
        }
        _tr.ReceiveOnTriggerStay2D(collider, _myCollider);
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