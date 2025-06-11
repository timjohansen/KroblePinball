using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.InputSystem;

public class CoinSpawner : EventSender, INeedReset
{
    public GameObject spawnObj;
    public float initialSpawnInterval = 10f;
    private float _currentSpawnInterval;
    public int groupsPerInterval = 2;
    private float _spawnTimer;
    
    public EventSender spawnAllCoinsTrigger;
    public AudioClip allCoinsSound;
    
    private SplineContainer[] _splines;
    private int[] _maxCoinsInGroup;
    private int[] _coinsCollectedInGroup;
    UnityEvent<int> _destroyGroupEvent;
    
    void Start()
    {
        _splines = GetComponentsInChildren<SplineContainer>();
        int splineCount = _splines.Length;
        _coinsCollectedInGroup = new int[splineCount];
        _maxCoinsInGroup = new int[splineCount];
        for (int i = 0; i < splineCount; i++)
        {
            _coinsCollectedInGroup[i] = 0;
            // Gets the number of coins from the last two characters of the spline's name
            _maxCoinsInGroup[i] = int.Parse(_splines[i].gameObject.name[^2..]); 
        }
        _destroyGroupEvent = new UnityEvent<int>();

        if (spawnAllCoinsTrigger)
        {
            spawnAllCoinsTrigger.GetBoardEvent().AddListener(SpawnAllGroups);
        }

        _spawnTimer = initialSpawnInterval;
        _currentSpawnInterval = initialSpawnInterval;
    }

    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Play || GM.inst.mode == GM.GameMode.Multiball)
        {
            _spawnTimer -= Time.deltaTime;
        }
            
        if (_spawnTimer <= 0)
        {
            for (int i = 0; i < groupsPerInterval; i++)
            {
                SpawnLeastPopulatedGroup();
            }
            _spawnTimer = initialSpawnInterval;
        }
    }

    public void ResetForNewGame()
    {
        for (int i = 0; i < _splines.Length; i++)
        {
            _destroyGroupEvent.Invoke(i);
            _coinsCollectedInGroup[i] = 0; 
        }
        
        _currentSpawnInterval = initialSpawnInterval;
        _spawnTimer = initialSpawnInterval;
    }

    void SpawnLeastPopulatedGroup()
    {
        int leastPercent = 0;
        List<int> groups = new List<int>();
        
        for (int i = 0; i < _splines.Length; i++)
        {
            int percentCollected = (int)((_coinsCollectedInGroup[i] / (float)_maxCoinsInGroup[i]) * 100f);
            if (percentCollected > leastPercent)
            {
                leastPercent = percentCollected;
            }
        }

        for (int i = 0; i < _splines.Length; i++)
        {
            int percentCollected = (int)((_coinsCollectedInGroup[i] / (float)_maxCoinsInGroup[i]) * 100f); 
            if (percentCollected == leastPercent)
            {
                groups.Add(i);
            }
        }
        SpawnGroup(Random.Range(0, groups.Count));    
    }
    
    void SpawnGroup(int group)
    {
        _destroyGroupEvent.Invoke(group);
        _coinsCollectedInGroup[group] = 0; 
        float fraction = 1f / _maxCoinsInGroup[group];
        for (int j = 0; j < _maxCoinsInGroup[group]; j++)
        {            
            Vector3 spawnPos = _splines[group].Spline.EvaluatePosition(j * fraction);
            GameObject newObj = Instantiate(spawnObj, spawnPos, Quaternion.identity);
            Coin ht = newObj.GetComponent<Coin>();
            ht.SetGroupNum(group);
            ht.GetPickupEvent().AddListener(TokenCollected);
            ht.SetDestroyEvent(_destroyGroupEvent);
        }
    }

    void SpawnAllGroups(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, allCoinsSound));
        for (int i = 0; i < _splines.Length; i++)
        {
            SpawnGroup(i);
        }
    }

    void TokenCollected(int group)
    {
        boardEvent.Invoke(new EventInfo(this, EventType.AddPoints, GM.inst.coinPointValue));
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "coin"));
    }

    public float GetSpawnInterval()
    {
        return _currentSpawnInterval;
    }
    
    public void SetSpawnInterval(float newSpawnInterval)
    {
        if (newSpawnInterval <= 0f)
        {
            Debug.LogWarning("Coin spawn interval must be greater than zero. Defaulting to 10s");
            newSpawnInterval = 10f;
        }
        _currentSpawnInterval = newSpawnInterval;
    }
}
