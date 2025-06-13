using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Ball : MonoBehaviour
{
    public Ball2D ball2D;
    public GameObject ball3D;
    public Rigidbody2D rb2D { get; private set; }
    public float radius { get; private set; }
    public float speed { get; private set; }

    public StoredBallInfo storedInfo { get; private set; }

    private AudioSource _audioSource;
    private float _soundSpeedThreshold = 2.5f;          // At what speed does the ball rolling sound become audible
    private bool _paused;                               // Not currently used
    
    void Awake()
    {
        rb2D = ball2D.GetComponent<Rigidbody2D>();
        radius = ball2D.GetComponent<CircleCollider2D>().radius;
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        GM.inst.GetPauseEvent().AddListener(GamePause);
    }
    
    void Update()
    {
        if (_paused)
            return;
        // Place the 3D ball based on the 2D ball's position, and stick it to the ground.
        Physics.Raycast(ball3D.transform.position + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hit, 10f, rb2D.includeLayers);
        
        ball3D.transform.position = new Vector3(ball2D.transform.position.x - GM.inst.offset2D.x, hit.point.y + radius,
            ball2D.transform.position.y - GM.inst.offset2D.z);
    }

    private void FixedUpdate()
    {
        if (_paused)
            return;
        
        if (rb2D.velocity.magnitude > GM.inst.ballSpeedCap && rb2D.bodyType == RigidbodyType2D.Dynamic)
        {
            rb2D.velocity = rb2D.velocity.normalized * GM.inst.ballSpeedCap;
        }
        speed = rb2D.velocity.magnitude;

        if (speed > _soundSpeedThreshold)
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
        // Saves the ball's current state and disables physics simulation
        storedInfo = new StoredBallInfo
        {
            Position = hitPos,
            Velocity = rb2D.velocity,
            FlipperValue = fv
        };
                
        if (rb2D.bodyType != RigidbodyType2D.Kinematic)
        {
            rb2D.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public StoredBallInfo MakeDynamic()
    {
        // Re-enables simulation and returns the previously stored state information 
        if (rb2D.bodyType != RigidbodyType2D.Dynamic)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic;
        }
        return storedInfo;
    }
    
    public void ChangeLayer(string layerName)
    {
        rb2D.includeLayers = LayerMask.GetMask(layerName);
        List<string> exList = new();
        foreach (string item in GM.inst.collisionLayers)
        {
            if (item != layerName)
            {
                exList.Add(item);
            }
        }
        rb2D.excludeLayers = LayerMask.GetMask(exList.ToArray());
    }

    public void SetPosition2D(Vector2 position)
    {
        ball2D.transform.position = position;
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        rb2D.velocity = velocity;
    }
    public void AddImpulseForce(Vector2 force)
    {
        rb2D.AddForce(force, ForceMode2D.Impulse);
        Debug.DrawRay(rb2D.position, force, Color.cyan, 3f);
    }
    
    void GamePause(bool isPaused)
    {
        if (isPaused)
        {
            _paused = true;
            rb2D.simulated = false;
        }
        else
        {
            _paused = false;
            rb2D.simulated = true;
        }
    }

    public struct StoredBallInfo
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float FlipperValue;
    }
}
