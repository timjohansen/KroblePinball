using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class GameOver : MonoBehaviour
{
    
    public Image gameOverImage;
    public TMPro.TMP_Text scoreText;
    private RectTransform _canvasRect;
    private RectTransform _myRect;
    
    
    private float _animInTimer;
    
    private float _gameOverYPos = 0;
    private float _scoreTextYPos = -150f;
    
    void Start()
    {
        _animInTimer = 1f;
        _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        _myRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        _myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _canvasRect.rect.width);
        _myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _canvasRect.rect.height);
        if (_animInTimer >= 0f)
        {
            _animInTimer = Mathf.MoveTowards(_animInTimer, 0f, Time.deltaTime);
            float invTime = 1f - _animInTimer;
            Vector3 pos = gameOverImage.rectTransform.localPosition;
            Vector3 logoStart = new Vector3(pos.x, _gameOverYPos + 250f, pos.z);
            Vector3 logoEnd = new Vector3(pos.x, _gameOverYPos, pos.z);
            gameOverImage.rectTransform.localPosition = Vector3.Lerp(logoStart, logoEnd, invTime);
            
            pos = scoreText.rectTransform.localPosition;
            Vector3 textStart = new Vector3(pos.x, _scoreTextYPos - 250f, pos.z);
            Vector3 textEnd = new Vector3(pos.x, _scoreTextYPos, pos.z);
            scoreText.rectTransform.localPosition = Vector3.Lerp(textStart, textEnd, invTime);
        }

        if (Keyboard.current.anyKey.wasPressedThisFrame || Touchscreen.current.primaryTouch.press.isPressed)
        {
            GM.inst.StartNewGame();
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _animInTimer = 1f;
        // GetComponent<AudioSource>().PlayOneShot(gameOverSound);
    }
}
