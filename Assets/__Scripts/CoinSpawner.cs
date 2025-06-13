using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.Serialization;

public class CoinSpawner : EventSender, INeedReset
{
    public GameObject coinPrefab;
    public EventSender spawnAllCoinsTrigger;
    public AudioClip spawnAllCoinsClip;
    public float initialSpawnInterval = 10f;
    public float spawnInterval { get; set; }
    public int groupsPerInterval { get; set; } = 2;
    private float _spawnTimer;
    
    private SplineContainer[] _splines;
    private int[] _maxCoinsInGroup;
    private int[] _coinsCollectedInGroup;
    private UnityEvent<int> _destroyGroupEvent;
    
    void Start()
    {
        _spawnTimer = initialSpawnInterval;
        spawnInterval = initialSpawnInterval;
        
        _splines = GetComponentsInChildren<SplineContainer>();
        int splineCount = _splines.Length;
        if (splineCount == 0)
        {
            Debug.LogWarning("No coin splines were found", gameObject);
        }
        _coinsCollectedInGroup = new int[splineCount];
        _maxCoinsInGroup = new int[splineCount];
        
        for (int i = 0; i < splineCount; i++)
        {
            _coinsCollectedInGroup[i] = 0;
            // Gets the number of coins from the last two characters of the spline object's name
            _maxCoinsInGroup[i] = int.Parse(_splines[i].gameObject.name[^2..]); 
        }
        _destroyGroupEvent = new UnityEvent<int>();

        if (spawnAllCoinsTrigger)
        {
            spawnAllCoinsTrigger.GetBoardEvent().AddListener(SpawnAllGroups);
        }
    }
    
    public void ResetForNewGame()
    {
        for (int i = 0; i < _splines.Length; i++)
        {
            _destroyGroupEvent.Invoke(i);
            _coinsCollectedInGroup[i] = 0; 
        }
        
        spawnInterval = initialSpawnInterval;
        _spawnTimer = initialSpawnInterval;
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
    
    void SpawnLeastPopulatedGroup()
    {
        // Selects the coin group with the least already-collected coins to be spawned. If multiple groups are
        // equal (such as any never-spawned groups sitting at zero percent), they will be chosen from randomly.
        // The percentage is calculated with integers to ensure ties aren't missed due to rounding errors. 
        
        int mostPercentCollected = 0;
        List<int> groups = new List<int>();
        
        for (int i = 0; i < _splines.Length; i++)
        {
            int percentCollected = (int)((_coinsCollectedInGroup[i] / (float)_maxCoinsInGroup[i]) * 100f);
            if (percentCollected > mostPercentCollected)
            {
                mostPercentCollected = percentCollected;
            }
        }

        for (int i = 0; i < _splines.Length; i++)
        {
            int percentCollected = (int)((_coinsCollectedInGroup[i] / (float)_maxCoinsInGroup[i]) * 100f); 
            if (percentCollected == mostPercentCollected)
            {
                groups.Add(i);
            }
        }
        SpawnGroup(Random.Range(0, groups.Count));    
    }
    
    void SpawnGroup(int group)
    {
        _destroyGroupEvent.Invoke(group);   // We aren't individually tracking which coins have been collected already,
                                            // so destroying them first is necessary to avoid duplicates.
        _coinsCollectedInGroup[group] = 0;
        
        float distanceBetweenCoins = 1f / (_maxCoinsInGroup[group] + 1);
        for (int j = 0; j < _maxCoinsInGroup[group]; j++)
        {            
            // Spawn each coin equally spaced along its spline and initialize
            Vector3 spawnPos = _splines[group].Spline.EvaluatePosition(j * distanceBetweenCoins);
            
            GameObject newObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity); // TODO: set up pooling
            Coin coin = newObj.GetComponent<Coin>();
            coin.SetGroupNum(group);
            coin.GetPickupEvent().AddListener(OnTokenCollected);
            coin.SetDestroyEvent(_destroyGroupEvent);
        }
    }

    void SpawnAllGroups(EventInfo info)
    {
        if (info.Type != EventType.Trigger)
        {
            return;
        }
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, spawnAllCoinsClip));
        for (int i = 0; i < _splines.Length; i++)
        {
            SpawnGroup(i);
        }
    }

    void OnTokenCollected(int group)
    {
        boardEvent.Invoke(new EventInfo(this, EventType.AddPoints, GM.inst.coinPointValue));
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "coin"));
    }

    public float GetSpawnInterval()
    {
        return spawnInterval;
    }
    
    public void SetSpawnInterval(float newSpawnInterval)
    {
        if (newSpawnInterval <= 0f)
        {
            Debug.LogWarning("Coin spawn interval must be greater than zero. Defaulting to 10s");
            newSpawnInterval = 10f;
        }
        spawnInterval = newSpawnInterval;
    }
}
