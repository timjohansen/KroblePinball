using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BallDispenser : EventSender, ITriggerReceiver, INeedReset
{
    public GameObject ballPrefab;
    public GameObject ballSpawnLoc;
    public int launcherStrength;
    private bool _ballInChute;
    List<GameObject> _activeBalls = new();
    Stack<GameObject> _inactiveBalls = new();
    private int _initialBallObjectCount;
    private int _totalBallCount;
    private UnityEvent<GameObject> _ballDespawnEvent;
    public BoxCollider2D chuteArea;

    private int _queuedBalls = 0;

    private bool _multiball;
    public int ballsInPlay { get; private set; }

    public Material[] ballMaterials;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < _initialBallObjectCount; i++)
        {
            CreateBallObject();
        }

        _ballDespawnEvent = new UnityEvent<GameObject>();
    }

    void Start()
    {
        GM.inst.modeChangeEvent.AddListener(OnModeChange);
        ballsInPlay = 0;
    }

    public void ResetForNewGame()
    {
        List<GameObject> objsToDespawn = new List<GameObject>(_activeBalls);
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
        _totalBallCount++;
    }

    public void SpawnBall()
    {
        ballsInPlay++;
    }

    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Play || GM.inst.mode == GM.GameMode.Multiball)
        {
            if (_activeBalls.Count + _queuedBalls < ballsInPlay)
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
        ballToSpawn.GetComponent<Ball>().ball2D.transform.position = ballSpawnLoc.transform.position;
        ballToSpawn.GetComponent<Ball>().ChangeLayer("Layer1");
        _activeBalls.Add(ballToSpawn);
        _queuedBalls--;
    }
    
    public void DespawnBall(GameObject ballToDespawn)
    {
        ballsInPlay--;
        _activeBalls.Remove(ballToDespawn);
        _inactiveBalls.Push(ballToDespawn);
        ballToDespawn.gameObject.GetComponent<Ball>().ball3D.GetComponent<TrailRenderer>().emitting = false;
        ballToDespawn.SetActive(false);
        _ballDespawnEvent.Invoke(ballToDespawn);
    }

    public List<GameObject> GetActiveBalls()
    {
        return _activeBalls;
    }

    public UnityEvent<GameObject> GetBallDespawnEvent()
    {
        return _ballDespawnEvent;
    }

    public void SetBallMaterial(int index)
    {
        if (index < 0 || index >= ballMaterials.Length)
        {
            Debug.LogError("Ball material index out of range");
            return;
        }

        foreach (GameObject ball in _activeBalls)
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
