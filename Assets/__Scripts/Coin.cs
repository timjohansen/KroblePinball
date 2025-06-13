using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Coin : EventSender

{
    public GameObject pickupTriggerPrefab;
    public int groupNum; // Not currently used, but here in case there's ever a need to identify a coin's origin

    private UnityEvent<int> _pickupEvent = new UnityEvent<int>();
    private GameObject _pickupTriggerInst;
    private bool _collected;


    void Start()
    {
        Vector3 offset = GM.inst.offset2D;
        Vector3 triggerPos = 
            new(transform.position.x + offset.x, transform.position.z + offset.z, transform.position.y + offset.y);
        _pickupTriggerInst =
            Instantiate(pickupTriggerPrefab, triggerPos, Quaternion.identity);
        _pickupTriggerInst.GetComponent<RolloverTrigger>().GetBoardEvent().AddListener(Collect);
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
            Debug.LogWarning("Attempted to collect already collected token", gameObject);
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
        if (_pickupTriggerInst)
        {
            Destroy(_pickupTriggerInst.gameObject);
        }

        Destroy(gameObject);
    }
}