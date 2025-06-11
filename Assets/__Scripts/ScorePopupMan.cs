using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventSender;
public class ScorePopupMan : MonoBehaviour
{
    public GameObject scorePopup;
    
    void Start()
    {
        EventSender[] senders = FindObjectsByType<EventSender>(FindObjectsSortMode.None);
        foreach (EventSender sender in senders)
        {
            sender.GetBoardEvent().AddListener(HandleBoardEvent);
        }
    }

    void HandleBoardEvent(EventInfo info)
    {
        if (info.Type != EventSender.EventType.AddPoints)
            return;

        Vector3 popupPos;
        if (info.Position3D.HasValue)
            popupPos = info.Position3D.Value;
        else if (info.Position2D.HasValue)
            popupPos = new Vector3(info.Position2D.Value.x, 0f, info.Position2D.Value.y) - GM.inst.offset2D;
        else
            return;
        
        GameObject popupObj = Instantiate(scorePopup, popupPos, Quaternion.identity);
        popupObj.GetComponent<ScorePopup>().SetPointValue((int)info.Data);
    }
}
