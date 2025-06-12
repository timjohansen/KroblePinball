using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class TitleScreen : MonoBehaviour
{
    private RectTransform _canvasRect;
    private RectTransform _myRect;
    public Image logo;
    public TMPro.TMP_Text pushStartText;
    public TMPro.TMP_Text controlsText;
    private float _animInTimer;
    
    private float _logoEndYPos = 100f;
    private float _pushStartTextYPos = -100f;
    void Start()
    {
        _animInTimer = 1f;
        _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        _myRect = GetComponent<RectTransform>();

        if (Application.isMobilePlatform)
        {
            pushStartText.text = "Tap To Start";
            controlsText.gameObject.SetActive(false);
        }
        else
        {
            pushStartText.text = "Press any key to start";
            controlsText.gameObject.SetActive(true);
        }
    }
    
    void Update()
    {
        
        _myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _canvasRect.rect.width);
        _myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _canvasRect.rect.height);
        if (_animInTimer >= 0f)
        {
            _animInTimer = Mathf.MoveTowards(_animInTimer, 0f, Time.deltaTime);
            float invTime = 1f - _animInTimer;
            Vector3 pos = logo.rectTransform.localPosition;
            Vector3 logoStart = new Vector3(pos.x, _logoEndYPos + 250f, pos.z);
            Vector3 logoEnd = new Vector3(pos.x, _logoEndYPos, pos.z);
            logo.rectTransform.localPosition = Vector3.Lerp(logoStart, logoEnd, invTime);
            
            pos = pushStartText.rectTransform.localPosition;
            Vector3 textStart = new Vector3(pos.x, _pushStartTextYPos - 250f, pos.z);
            Vector3 textEnd = new Vector3(pos.x, _pushStartTextYPos, pos.z);
            pushStartText.rectTransform.localPosition = Vector3.Lerp(textStart, textEnd, invTime);
        }

        if (GM.inst.inputMan.anyKeyPressed)
        {
            GM.inst.StartNewGame();
            gameObject.SetActive(false);
        }
    }
}
