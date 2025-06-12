using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TouchCanvas : MonoBehaviour
{
    public Image musicCrossImage;
    
    // Automatically hides itself if no touch devices are detected
    void Awake()
    {
        if (Touchscreen.current == null || !Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }   
    }

    public void SetMusicIconState(bool state)
    {
        musicCrossImage.gameObject.SetActive(state);
    }
}
