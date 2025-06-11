using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpinnerMan : EventSender
{
    public EventSender upgradeTrigger;
    public Spinner[] spinnerObjs;
    public int levelUpPointBonus = 500;
    public bool testMode;
    void Start()
    {
        upgradeTrigger.GetBoardEvent().AddListener(OnUpgradeTrigger);
    }
    
    void Update()
    {
        if (testMode && Keyboard.current.hKey.wasPressedThisFrame)
        {
            OnUpgradeTrigger(new EventInfo(EventType.Trigger));
        }
    }
    
    void OnUpgradeTrigger(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
            return;
        foreach (Spinner sp in spinnerObjs)
        {
            sp.Upgrade(1);
        }
        
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {

                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Horizontal,
                    2f, 0, "Spinners", TextureWrapMode.Clamp),
                
                new (DotMatrixDisplay.AnimType.ScrollInOut, 
                    DotMatrixDisplay.AnimOrient.Horizontal,
                    2f, 0, "Upgraded", TextureWrapMode.Clamp),
            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "success_minor"));
        boardEvent.Invoke(new EventInfo(this, EventType.AddPoints, levelUpPointBonus));
    }
}
