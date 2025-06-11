using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSpeechBubble : MonoBehaviour
{
    public Image leftCap;
    public Image center;
    public Image rightCap;
    public Image arrow;
    public TMP_Text mainTextObj;
    private TMP_Text _dummyTextObj;
    public float characterInterval = .25f;
    public AudioClip startTextSound;
    public AudioClip characterSound;
    
    private string _fullText = "";
    private string _currentText = "";
    private float _characterTimer;

    private float _currentAnimInTimer;
    private float _maxAnimInTime;
    private float _centerTargetScale;
    private float _rightCapStartPos;
    private float _rightCapTargetPos;
    private bool _bubbleAnimationReady;

    private float _fadeInTimer;
    private float _fadeOutTimer;
    
    private AudioSource _audioSource;
    
    
    
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        // Create a duplicate "dummy" text object offscreen that will be used to get the width of the text 
        GameObject dummyObj = Instantiate(mainTextObj.gameObject, transform.position, Quaternion.identity);
        dummyObj.GetComponent<RectTransform>().SetParent(transform);
        _dummyTextObj = dummyObj.GetComponent<TMP_Text>();
        _dummyTextObj.rectTransform.anchoredPosition =
            new Vector2(mainTextObj.rectTransform.anchoredPosition.x, mainTextObj.rectTransform.anchoredPosition.y - 1000);
        _dummyTextObj.rectTransform.localScale = Vector3.one;
        
        _rightCapStartPos = rightCap.rectTransform.anchoredPosition.x;
        
        // Hide
        mainTextObj.text = "";
        Color color = new Color(1f, 1f, 1f, 0f);
        leftCap.color = color;
        center.color = color;
        rightCap.color = color;
        arrow.color = color;
    }

    void Update()
    {
        if (_currentText.Length < _fullText.Length)
        {
            if (_characterTimer < characterInterval)
            {
                _characterTimer += Time.deltaTime;
            }
            else
            {
                _currentText += _fullText[_currentText.Length];
                mainTextObj.text = _currentText;
                _characterTimer = 0f;
                if (characterSound)
                    _audioSource.PlayOneShot(characterSound);
            }
        }

        if (_currentAnimInTimer > 0f)
        {
            if (_dummyTextObj.renderedWidth < 1f)
            {
                // Dummy text hasn't been rendered yet, so we don't know the total width yet
                return;
            }

            if (!_bubbleAnimationReady)
            {
                _rightCapTargetPos = _dummyTextObj.renderedWidth + 100f;
                _centerTargetScale = _dummyTextObj.renderedWidth / 100f;
                _bubbleAnimationReady = true;
            }
            _currentAnimInTimer = Mathf.MoveTowards(_currentAnimInTimer, 0f, Time.deltaTime);
            float timerProgress = Mathf.InverseLerp(_maxAnimInTime, 0f, _currentAnimInTimer);
            float rightPos = Mathf.Lerp(_rightCapStartPos, _rightCapTargetPos, timerProgress);
            float centerScale = Mathf.Lerp(0f, _centerTargetScale, timerProgress);
            
            rightCap.rectTransform.anchoredPosition = new Vector2(rightPos, 0f);
            center.rectTransform.localScale = new Vector3(centerScale , 1f, 1f);
            arrow.rectTransform.localScale = new Vector3(timerProgress, timerProgress, timerProgress);
        }

        if (_fadeInTimer > 0f)
        {
            _fadeInTimer -= Time.deltaTime;
            
            float alpha = 1f - Mathf.Clamp01(_fadeInTimer);
            Color color = new Color(1f, 1f, 1f, alpha);
            leftCap.color = color;
            center.color = color;
            rightCap.color = color;
            arrow.color = color;
        }

        if (_fadeOutTimer > 0f)
        {
            _fadeOutTimer -= Time.deltaTime;
            if (_fadeOutTimer < 1f)
            {
                float alpha = Mathf.Clamp01(_fadeOutTimer);
                Color color = new Color(1f, 1f, 1f, alpha);
                leftCap.color = color;
                center.color = color;
                rightCap.color = color;
                arrow.color = color;
            }
        }
    }

    public void SetNewText(string text)
    {
        _fullText = text;
        _currentText = "";
        _dummyTextObj.text = _fullText;
        _characterTimer = characterInterval; 
        _maxAnimInTime = .33f;
        _currentAnimInTimer = _maxAnimInTime;
        _bubbleAnimationReady = false;
        
        rightCap.rectTransform.anchoredPosition = new Vector2(_rightCapStartPos, 0f);
        center.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        
        Color color = new Color(1f, 1f, 1f, 0f);
        leftCap.color = color;
        center.color = color;
        rightCap.color = color;
        arrow.color = color;
        arrow.rectTransform.localScale = new Vector3(0f, 0f, 0f);
        
        _fadeInTimer = .5f;
        _fadeOutTimer = 3.5f;
        
        if (startTextSound)
            _audioSource.PlayOneShot(startTextSound);
    }
}
