using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;


public class InputMan : MonoBehaviour
{
    // Translates traditional keyboard/gamepad input and touchscreen input to a unified set of fields. 
    
    public InputActionReference leftFlipperAction;
    public InputActionReference rightFlipperAction;
    public InputActionReference leftNudgeAction;
    public InputActionReference rightNudgeAction;
    public InputActionReference upNudgeAction;
    public InputActionReference musicToggleAction;
    public InputActionReference fullscreenAction;
    public InputActionReference exitFullscreenAction;
    public InputActionReference anyKeyAction;

    public EventSystem canvasEventSystem;
    public GraphicRaycaster canvasRaycaster;
    private PointerEventData _eventData;
    
    public bool leftFlipperPressed { get; private set; }
    public bool rightFlipperPressed { get; private set; }
    public bool upNudgePressed { get; private set; }
    public bool leftNudgePressed { get; private set; }
    public bool rightNudgePressed { get; private set; }
    public bool musicTogglePressed { get; private set; }

    public bool fullscreenPressed { get; private set; }
    public bool exitFullscreenPressed { get; private set; }
    public bool anyKeyPressed { get; private set; }

    // public List<Vector2> touches { get; } = new List<Vector2>();

    void Awake()
    {
        leftFlipperAction.action.Enable();
        rightFlipperAction.action.Enable();
        leftNudgeAction.action.Enable();
        rightNudgeAction.action.Enable();
        upNudgeAction.action.Enable();
        musicToggleAction.action.Enable();
        fullscreenAction.action.Enable();
        exitFullscreenAction.action.Enable();
        anyKeyAction.action.Enable();
    }
    
    void Update()
    {
        leftFlipperPressed = false;
        rightFlipperPressed = false;
        upNudgePressed = false;
        leftNudgePressed = false;
        rightNudgePressed = false;
        musicTogglePressed = false;
        fullscreenPressed = false;
        exitFullscreenPressed = false;
        
        anyKeyPressed = false;
        
        if (Keyboard.current != null || Gamepad.current != null)
        {
            ProcessInputActions();
        }
        if (Touchscreen.current != null)
        {
            ProcessTouch();
        }
    }

    private void ProcessInputActions()
    {
        anyKeyPressed = anyKeyAction.action.IsPressed();
        leftFlipperPressed = leftFlipperAction.action.IsPressed();
        rightFlipperPressed = rightFlipperAction.action.IsPressed();
        upNudgePressed = upNudgeAction.action.IsPressed();
        leftNudgePressed = leftNudgeAction.action.IsPressed();
        rightNudgePressed = rightNudgeAction.action.IsPressed();
        
        musicTogglePressed = musicToggleAction.action.WasPressedThisFrame();
        fullscreenPressed = fullscreenAction.action.WasPressedThisFrame();
        exitFullscreenPressed = exitFullscreenAction.action.WasPressedThisFrame();
    }

    private void ProcessTouch()
    {
        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.IsPressed())
            {
                continue;
            }
            anyKeyPressed = true;
            
            _eventData = new PointerEventData(canvasEventSystem)
            {
                position = touch.position.value
            };
            List<RaycastResult> results = new List<RaycastResult>();
            canvasRaycaster.Raycast(_eventData, results);
            foreach (RaycastResult result in results)
            {
                switch (result.gameObject.name)
                {
                    case "LeftFlipper":
                        leftFlipperPressed = true;
                        break;
                    case "RightFlipper":
                        rightFlipperPressed = true;
                        break;
                    case "LeftUpNudge":
                    case "RightUpNudge":
                        upNudgePressed = true;
                        break;
                    case "LeftNudge":
                        leftNudgePressed = true;
                        break;
                    case "RightNudge":
                        rightNudgePressed = true;
                        break;
                    case "FullscreenToggle":
                        fullscreenPressed = true;
                        break;
                    case "MusicToggle":
                        musicTogglePressed = true;
                        break;
                }
            }
        }
    }
    
}
