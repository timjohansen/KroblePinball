using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    public TMP_Text text;
    Material _textMat;
    private int _pointValue;
    private float _lifetime;
    private float _initialLifetime = 1f;
    private float _scaleVal;

    void Start()
    {
        _textMat = new Material(text.material);
        text.material = _textMat;
    }
    void Update()
    {
        if (_lifetime < 0f)
        {
            // TODO: set up pooling
            Destroy(gameObject);
            return;
        }        
        _lifetime -= Time.deltaTime;
        text.color = new Color(text.color.r, text.color.g, text.color.b,1f ); // _lifetime / _initialLifetime 
        transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime, transform.position.z);
    }

    public void SetPointValue(int pointValue)
    {
        _pointValue = pointValue;
        _lifetime = _initialLifetime;
        text.text = _pointValue.ToString();
        _scaleVal = Mathf.InverseLerp(0f, 1000f, _pointValue);
        
        text.fontSize = Mathf.Lerp(1f, 5f, _scaleVal);
    }
}
