using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BallDispenser : EventSender, ITriggerReceiver, INeedReset
{
    public GameObject ballPrefab;           // Prefab containing an object with the Ball class
    public GameObject ballSpawnLoc;         // Empty object used to easily move the spawn location around
    public int launcherStrength;            // How much force is applied in the upward direction on launch
    
    public int ballsInPlay { get; private set; }
    public List<GameObject> activeBalls { get; private set; } = new();
    private Stack<GameObject> _inactiveBalls = new();   // Disabled ball objects waiting to be spawned 
    private int _initialBallObjectCount = 5;            // How many pooled ball objects to initially create

    public UnityEvent<GameObject> ballDespawnEvent { get; private set; }    // Needed by the flippers.
                                                        
    private int _queuedBalls = 0;           // Balls waiting to be spawned.
    private bool _ballInChute;              // The script won't spawn another ball if one is currently in the chute.

    public Material[] ballMaterials;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < _initialBallObjectCount; i++)
        {
            CreateBallObject();
        }

        ballDespawnEvent = new UnityEvent<GameObject>();
    }

    void Start()
    {
        GM.inst.modeChangeEvent.AddListener(OnModeChange);
        ballsInPlay = 0;
    }

    public void ResetForNewGame()
    {
        List<GameObject> objsToDespawn = new List<GameObject>(activeBalls);
        foreach (var obj in objsToDespawn)
        {
            DespawnBall(obj);
        }

        ballsInPlay = 0;
    }

    private void CreateBallObject()
    {
        GameObject newBall = Instantiate(ballPrefab, ballSpawnLoc.transform.position, Quaternion.identity);
        newBall.SetActive(false);
        _inactiveBalls.Push(newBall);
    }

    public void SpawnBall()
    {
        ballsInPlay++;
    }

    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Play || GM.inst.mode == GM.GameMode.Multiball)
        {
            if (activeBalls.Count + _queuedBalls < ballsInPlay)
            {
                _queuedBalls++;
            }
        }

        if (_queuedBalls > 0)
        {
            ProcessQueue();
        }
    }

    void ProcessQueue()
    {
        if (_inactiveBalls.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                CreateBallObject();
            }
        }

        if (_ballInChute)
            return;

        GameObject ballToSpawn = _inactiveBalls.Pop();
        ballToSpawn.SetActive(true);
        ballToSpawn.gameObject.GetComponent<Ball>().ball3D.GetComponent<TrailRenderer>().emitting = true;
        ballToSpawn.GetComponent<Ball>().SetPosition2D(ballSpawnLoc.transform.position);
        ballToSpawn.GetComponent<Ball>().ChangeLayer("Layer1");
        activeBalls.Add(ballToSpawn);
        _queuedBalls--;
    }
    
    public void DespawnBall(GameObject ballToDespawn)
    {
        ballsInPlay--;
        activeBalls.Remove(ballToDespawn);
        _inactiveBalls.Push(ballToDespawn);
        ballToDespawn.gameObject.GetComponent<Ball>().ball3D.GetComponent<TrailRenderer>().emitting = false;
        ballToDespawn.SetActive(false);
        ballDespawnEvent.Invoke(ballToDespawn);
    }
    
    public void SetBallMaterial(int index)
    {
        if (index < 0 || index >= ballMaterials.Length)
        {
            Debug.LogError("Ball material index out of range");
            return;
        }

        foreach (GameObject ball in activeBalls)
        {
            ball.GetComponent<Ball>().ball3D.GetComponent<Renderer>().material = ballMaterials[index];
        }

        foreach (GameObject ball in _inactiveBalls)
        {
            ball.GetComponent<Ball>().ball3D.GetComponent<Renderer>().material = ballMaterials[index];
        }
    }

    void OnModeChange()
    {
        if (GM.inst.mode == GM.GameMode.Multiball)
        {
            ballsInPlay = 3;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            _ballInChute = true;
        }   
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            _ballInChute = false;
        }   
    }
    
    public void ReceiveOnTriggerEnter2D(Collider2D other, Collider2D self)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;

        Ball ballComp = other.gameObject.GetComponentInParent<Ball>();
        ballComp.SetVelocity(new Vector2(0f, launcherStrength));
        boardEvent.Invoke(new EventInfo(this, EventType.Trigger));
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, "plunger"));
    
    }

    public void ReceiveOnTriggerExit2D(Collider2D other, Collider2D self)
    {
        // Not needed
    }

    public void ReceiveOnTriggerStay2D(Collider2D other, Collider2D self)
    {
        // Not needed
    }
}
