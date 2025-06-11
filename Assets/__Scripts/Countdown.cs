using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public Sprite[] numberSprites;
    public Sprite goSprite;
    private Image _image;
    private int _currentIndex;
    public float timePerNumber;
    private float _timer;
    private bool _go;
    private bool _released;
    private AudioSource _audioSource;
    public AudioClip numberSound;
    public AudioClip goSound;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (_go)
        {
            if (!_released)
            {
                GM.inst.SetGameMode(GM.GameMode.Play);
                _released = true;
            }
            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _timer);
            }            
            if (_timer <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
        
        else if (_timer > 0f)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                _timer = 0f;
            }
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _image.sprite = goSprite;
                _image.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                _go = true;
                _timer = timePerNumber * 2f;
                _audioSource.PlayOneShot(goSound);
            }
            else
            {
                float scale = ((timePerNumber * 3) - _timer) % timePerNumber;
                scale *= .5f;
                _image.rectTransform.localScale = new Vector3(.5f + scale, .5f + scale, .5f + scale);
            
                int timerInt = (int)(_timer / timePerNumber);
                if (timerInt >= numberSprites.Length)
                {
                    Debug.LogWarning("Countdown exceeded image array");
                    return;
                }

                if (timerInt != _currentIndex)
                {
                    _image.sprite = numberSprites[timerInt];
                    _currentIndex = timerInt;
                    _audioSource.PlayOneShot(numberSound);
                }
            }
        }
    }

    public void StartCountdown()
    {
        _timer = timePerNumber * 3;
        _go = false;
        _released = false;
        _image = GetComponent<Image>();
        _image.sprite = numberSprites[^1];
        _image.color = Color.white;
        _image.rectTransform.localScale = new Vector3(.5f, .5f, .5f);
    }
}
