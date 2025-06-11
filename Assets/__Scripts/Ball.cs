using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Ball : MonoBehaviour
{
    public float radius;
    public Ball2D ball2D;
    public GameObject ball3D;
    private Rigidbody2D _rb2D;
    CircleCollider2D _col;
    
    public int currentLayer;
    private float _prevSpeed;
    public float currentSpeed;
    public bool touchingFlipper;

    bool paused;
    
    
    // bool switchRBType;
    // int switchRBDelay;
    RigidbodyType2D newRBType;

    public StoredBallInfo storedInfo;

    Vector2 positionBeforePhysicsUpdate;
    Vector2 velocityBeforePhysicsUpdate;

    public float soundSpeedThreshold = 2.5f;
    private AudioSource _audioSource;
    
    
    void Awake()
    {
        _rb2D = ball2D.GetComponent<Rigidbody2D>();
        radius = ball2D.GetComponent<CircleCollider2D>().radius;
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        GM.inst.GetPauseEvent().AddListener(GamePause);
    }
    
    void Update()
    {
        if (paused)
            return;
        // Place the 3D ball based on the 2D ball's position, and stick it to the ground.
        Physics.Raycast(ball3D.transform.position + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hit, 10f, _rb2D.includeLayers);
        
        ball3D.transform.position = new Vector3(ball2D.transform.position.x - GM.inst.offset2D.x, hit.point.y + radius,
            ball2D.transform.position.y - GM.inst.offset2D.z);
    }

    private void FixedUpdate()
    {
        if (paused)
            return;
        if (_rb2D.velocity.magnitude > GM.inst.ballSpeedCap && _rb2D.bodyType == RigidbodyType2D.Dynamic)
        {
            _rb2D.velocity = _rb2D.velocity.normalized * GM.inst.ballSpeedCap;
        }

        _prevSpeed = currentSpeed;
        currentSpeed = _rb2D.velocity.magnitude;

        /*if (_prevSpeed < soundSpeedThreshold && currentSpeed >= soundSpeedThreshold)
        {
            _audioSource.Play();
        }
        else if (_prevSpeed > soundSpeedThreshold && currentSpeed <= soundSpeedThreshold)
        {
            _audioSource.Stop();
        }

        if (currentSpeed < soundSpeedThreshold + 2f && _audioSource.isPlaying)
        {
            float volume = (soundSpeedThreshold + 2 - currentSpeed) * .5f;
            _audioSource.volume = volume;
        }*/
        if (currentSpeed > soundSpeedThreshold)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, 1f, 1f * Time.fixedDeltaTime);
        }
        else
        {
            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, 0f, 2f * Time.fixedDeltaTime);
            if (_audioSource.volume == 0f)
            {
                _audioSource.Stop();
            }
        }
    }
    
    public void MakeKinematic(float fv, Vector2 hitPos)
    {
        storedInfo = new StoredBallInfo
        {
            position = hitPos,
            velocity = _rb2D.velocity,
            flipperValue = fv
        };
                
        if (_rb2D.bodyType != RigidbodyType2D.Kinematic)
        {
            _rb2D.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public StoredBallInfo MakeDynamic()
    {
        if (_rb2D.bodyType != RigidbodyType2D.Dynamic)
        {
            _rb2D.bodyType = RigidbodyType2D.Dynamic;
        }
        
        return storedInfo;
    }
    
    public void ChangeLayer(string layerName)
    {
        _rb2D.includeLayers = LayerMask.GetMask(layerName);
        List<string> exList = new();
        foreach (string item in GM.inst.allLayers)
        {
            if (item != layerName)
            {
                exList.Add(item);
            }
        }
        _rb2D.excludeLayers = LayerMask.GetMask(exList.ToArray());
    }

    void GamePause(bool isPaused)
    {
        if (isPaused)
        {
            paused = true;
            _rb2D.simulated = false;
        }
        else
        {
            paused = false;
            _rb2D.simulated = true;
        }
    }

    public struct StoredBallInfo
    {
        public Vector2 position;
        public Vector2 velocity;
        public float flipperValue;
    }
    
    public void AddImpulseForce(Vector2 force)
    {
        _rb2D.AddForce(force, ForceMode2D.Impulse);
        Debug.DrawRay(_rb2D.position, force, Color.cyan, 3f);
    }

    public void SetVelocity(Vector2 velocity)
    {
        _rb2D.velocity = velocity;
    }
}
