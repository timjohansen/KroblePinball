using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Coin : EventSender

{
    UnityEvent<int> _pickupEvent = new UnityEvent<int>();    
    RolloverTrigger _pickupTrigger;
    bool _collected;
    public int groupNum;

    void Start()
    {
        Vector3 offset = GM.inst.offset2D;
        Vector3 triggerPos = new(transform.position.x + offset.x, transform.position.z + offset.z, transform.position.y + offset.y);
        _pickupTrigger = Instantiate(GM.inst.simpleTriggerPrefab, triggerPos, Quaternion.identity).GetComponent<RolloverTrigger>();
        _pickupTrigger.GetBoardEvent().AddListener(Collect);
    }
    
    void Update()
    {
        transform.Rotate(transform.up, 50f * Time.deltaTime);
    }

    void Collect(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        if (_collected)
        {
            Debug.LogError("Attempted to collect already collected token", gameObject);
            return;
        }
        _pickupEvent.Invoke(groupNum);
        _collected = true;
        DestroySelf();
        
    }

    public UnityEvent<int> GetPickupEvent()
    {
        return _pickupEvent;
    }

    public void SetGroupNum(int num)
    {
        groupNum = num;
    }

    public void SetDestroyEvent(UnityEvent<int> desEvent)
    {
        desEvent.AddListener(OnDestroyEvent);
    }

    void OnDestroyEvent(int group)
    {
        if (group == groupNum)
        {
            DestroySelf();
        }
    }
    
    void DestroySelf()
    {
        if (_pickupTrigger)
        {
            Destroy(_pickupTrigger.gameObject);
        }
        Destroy(gameObject);
        
    }
    
}
