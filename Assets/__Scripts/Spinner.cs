using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Spinner : EventSender, ITriggerReceiver, INeedReset
{

    public GameObject spinnerObj3D;
    public Transform axleTransform;
    public TMP_Text valueText;
    public float friction;
    public float hitVelocityMult;
    public int basePointValue;
    private int _level;
    private float _spinVelocity;
    private float _currentRotation;
    public Vector3 baseRot;
    
    public AudioClip[] clickSounds;
    public Renderer obj3DRenderer;
    public ParticleSystem upgradeParticles;
    public bool testMode;
    
    void Start()
    {
        _currentRotation = 0f;
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        SetLevel(0);
    }

    void Update()
    {
        if (_spinVelocity != 0f)
        {
            float angleToRotate = _spinVelocity * Time.deltaTime;
            float prevRotation = _currentRotation;;
            _currentRotation += angleToRotate;
            if (axleTransform)
            {
                axleTransform.localRotation = Quaternion.Euler(baseRot.x, _currentRotation, baseRot.z);                
            }
            else if (spinnerObj3D)
            {
                spinnerObj3D.transform.localRotation = Quaternion.Euler(baseRot.x, _currentRotation, baseRot.z);
            }
            

            for (int i = -180; i <= 180; i += 30)
            {
                if ((prevRotation < i && _currentRotation > i) || (prevRotation > i && _currentRotation < i))
                {
                    int index = Random.Range(0, clickSounds.Length);
                    boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, clickSounds[index]));
                }
            }
            
            if (_currentRotation > 180f)
            {
                Trigger();
                _currentRotation -= 180f;
            }
            else if (_currentRotation < -180f)
            {
                Trigger();
                _currentRotation += 180f;
            }
            float spinReduction = Mathf.Abs(_spinVelocity * (friction) * Time.deltaTime);
            _spinVelocity = Mathf.MoveTowards(_spinVelocity, 0f, spinReduction);
        }

        if (testMode && Keyboard.current.iKey.wasPressedThisFrame)
        {
            print("Upgrade");
            Upgrade(1);    
        }
    }
    
    void Trigger()
    {
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger));
        EventInfo pointEventInfo = new EventInfo(this, EventType.AddPoints, basePointValue * (_level + 1));
        
        if (spinnerObj3D)
        {
            pointEventInfo.Position3D = spinnerObj3D.transform.position;
        }
        
        boardEvent.Invoke(pointEventInfo);
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "spinner_points"));
    }
    public void Upgrade(int levels)
    {
        SetLevel(_level + levels);
        if (valueText)
        {
            valueText.text = valueText.text = (basePointValue * (_level + 1)).ToString();
        }
    }

    void SetLevel(int newLevel)
    {
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }
        
        obj3DRenderer.material.SetColor("_Color", GM.inst.levelColors[newLevel]);
        
        if (upgradeParticles && newLevel > 0)
        {
            var settings = upgradeParticles.main;
            Color newCol = GM.inst.levelColors[newLevel];
            newCol.a = 1f;
            settings.startColor = new ParticleSystem.MinMaxGradient(newCol);
            upgradeParticles.Play();
        }
        _level = newLevel;
    }
    
    public void ReceiveOnTriggerEnter2D(Collider2D collider, Collider2D myCollider)
    {
        
        if (!collider.gameObject.CompareTag("Ball"))
        {
            return;
        }
        Vector2 ballVelocity = collider.attachedRigidbody.velocity;
        Vector2 transformV2 =  new Vector2(myCollider.gameObject.transform.right.x, myCollider.gameObject.transform.right.y);
        
        _spinVelocity += ballVelocity.magnitude * Vector2.Dot(-ballVelocity.normalized, transformV2) * hitVelocityMult;
    }

    public void ReceiveOnTriggerExit2D(Collider2D collider, Collider2D myCollider)
    {

    }
    public void ReceiveOnTriggerStay2D(Collider2D collider, Collider2D myCollider)
    {

    }
    
}
