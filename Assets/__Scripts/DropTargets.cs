using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropTargets : EventSender, ICollisionReceiver, INeedReset
{
    public SkinnedMeshRenderer targetObj;
    public PolygonCollider2D[] targetCols;
    public Transform[] targetBones;    

    public float bounceForce = 250f;
    bool[] _targetDown;
    float _upZ;
    float _downZ;
    int _level;
    public ParticleSystem upgradeParticles;

    protected override void Awake()
    {
        base.Awake();
        _targetDown = new bool[targetCols.Length];
        _upZ = targetBones[0].localPosition.z;
        _downZ = _upZ - 0.155f;
    }
    
    private void Start()
    {        
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        RaiseAll();
    }

    private void Drop(int index)
    {
        Vector3 pos = targetBones[index].transform.localPosition;        
        targetBones[index].transform.localPosition = new Vector3(pos.x, pos.y, _downZ);
        UpdateMaterials();
    }

    private void Raise(int index)
    {
        Vector3 pos = targetBones[index].transform.localPosition;        
        targetBones[index].transform.localPosition = new Vector3(pos.x, pos.y, _upZ);
        UpdateMaterials();
    }

    private void RaiseAll()
    {
        for (int i = 0; i < targetCols.Length; i++)
        {
            _targetDown[i] = false;
            targetCols[i].isTrigger = false;
            Raise(i);            
        }
        UpdateMaterials();
    }

    private void UpdateMaterials()
    {
        List<Material> matList = new();
        targetObj.GetMaterials(matList);
        
        if (_level < GM.inst.levelColors.Length)
            matList[0].color = GM.inst.levelColors[_level];
        for (int i = 0; i < targetCols.Length; i++)
        {
            if (_targetDown[i])
            {
                matList[i + 1].SetInt("_LightOn", 1);
                matList[i + 1].SetInt("_BlinkType", 2);
            }
            else
            {
                matList[i + 1].SetInt("_LightOn", 1);
                matList[i + 1].SetInt("_BlinkType", 0);
            }
        }
        
        targetObj.SetMaterials(matList);
    }
    
    public void SetLevel(int newLevel)
    {
        if (newLevel >= GM.inst.levelColors.Length)
        {
            newLevel = GM.inst.levelColors.Length - 1;
        }
        _level = newLevel;
        UpdateMaterials();

        if (_level < GM.inst.levelColors.Length)
        {
            var settings = upgradeParticles.main;
            Color newCol = GM.inst.levelColors[_level];
            newCol.a = 1f;
            settings.startColor = new ParticleSystem.MinMaxGradient(newCol);
        }

        if (newLevel > 0)
            upgradeParticles.Play();
    }
    
    public void ReceiveOnCollisionEnter2D(Collision2D collision)
    {
        for (int i = 0; i < targetCols.Length; i++)
        {
            if (collision.otherCollider == targetCols[i])
            {
                _targetDown[i] = true;
                targetCols[i].isTrigger = true;
                Drop(i);

                Vector2 direction = -collision.GetContact(0).normal;
                collision.gameObject.GetComponentInParent<Ball>().AddImpulseForce(direction * bounceForce);

                
                EventInfo info = new EventInfo(this, EventType.Trigger)
                {
                    Position2D = collision.otherCollider.bounds.center
                };
                boardEvent.Invoke(info);
            }
        }
        bool allDown = true;
        for (int i = 0; i < targetCols.Length; i++)
        {
            if (!_targetDown[i])
            {
                allDown = false;
                break;
            }
        }
        if (allDown)
        {
            boardEvent.Invoke(new EventInfo(this, EventType.Trigger, name));
            RaiseAll();
        }
    }
    public void ReceiveOnCollisionExit2D(Collision2D collision)
    {

    }
    public void ReceiveOnCollisionStay2D(Collision2D collision)
    {

    }
}
