using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class SlotMachine : EventSender
{
    public EventSender[] eventSenders;
    public GameObject obj3D;
    private int _leftReelValue;
    private int _centerReelValue;
    private int _rightReelValue;
    
    private float _leftRotationRemaining;
    private float _centerRotationRemaining;
    private float _rightRotationRemaining;

    public bool isSpinning;
    public float spinSpeed = 500f;
    
    private AudioSource _reelAudioSource;
    public AudioClip reelStopSound;
    public AudioClip winSound;

    public bool testModePressS;
    
    private List<Material> _materials = new();
    
    void Start()
    {
        foreach (EventSender sender in eventSenders)
        {
            if (!sender)
                continue;
            sender.GetBoardEvent().AddListener(StartSpin); 
        }        
        _reelAudioSource = GetComponent<AudioSource>();
        obj3D.GetComponent<Renderer>().GetMaterials(_materials);
    }
    
    void Update()
    {
        if (testModePressS && Keyboard.current.sKey.wasPressedThisFrame)
            StartSpin(null);
        
        if (!isSpinning) 
            return;
            
        float amountToRotate = spinSpeed * Time.deltaTime;
        SpinReel(amountToRotate, ref _leftRotationRemaining,  _materials[0]);
        SpinReel(amountToRotate, ref _centerRotationRemaining, _materials[1]);
        SpinReel(amountToRotate, ref _rightRotationRemaining,  _materials[2]);

        if (_leftRotationRemaining == 0f && _centerRotationRemaining == 0f && _rightRotationRemaining == 0f)
        {
            isSpinning = false;
            _reelAudioSource.Stop();
            
            foreach (EventSender sender in eventSenders)
            {
                if (!sender)
                    continue;
                sender.ExternalReset();
            }
            
            if (_leftReelValue == _centerReelValue && _leftReelValue == _rightReelValue)
            {
                AwardPrize(_leftReelValue);
            }
        }
    }
    
    void StartSpin(EventInfo info)
    {
        if (isSpinning)
            return;
        
        int newLeft = (_leftReelValue + Random.Range(0, 4)) % 4;

        int newCenter = 12 + _centerReelValue;
        if (Random.Range(0, 2) == 0 || Keyboard.current.sKey.wasPressedThisFrame)
        {
            while (newCenter % 4 != newLeft)
            {
                newCenter += 1;
            }
        }
        else
        {
            newCenter = 12 + Random.Range(0, 4);
        }

        int newRight = 20 + _rightReelValue;
        if (Random.Range(0, 2) == 0 || Keyboard.current.sKey.wasPressedThisFrame)
        {
            while (newRight % 4 != newLeft)
            {
                newRight += 1;
            }            
        }
        else
        {
            newRight = 20 + Random.Range(0, 4);
        }
        newLeft += 8;
        
        _leftRotationRemaining = (newLeft - _leftReelValue) * .25f;
        _centerRotationRemaining = (newCenter - _centerReelValue) * .25f;
        _rightRotationRemaining = (newRight - _rightReelValue) * .25f;
        
        _leftReelValue = newLeft % 4;
        _centerReelValue = newCenter % 4;
        _rightReelValue = newRight % 4;
        
        isSpinning = true;
        
        _reelAudioSource.Play();
        if (reelStopSound)
            boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, reelStopSound));
    }
    
    void SpinReel(float amountToRotate, ref float rotationRemaining, Material reelMat)
    {
        float offset = reelMat.mainTextureOffset.y;
        
        if (rotationRemaining == 0f)
        {
            return;
        }
        if (rotationRemaining <= amountToRotate)
        {
            offset -= rotationRemaining;
            rotationRemaining = 0f;
            if (reelStopSound)
                boardEvent.Invoke(new EventInfo(this, EventType.PlaySound, reelStopSound));
        }
        else
        {
            offset -= amountToRotate;
            rotationRemaining -= amountToRotate;
        }
        reelMat.mainTextureOffset = new Vector2(0f, offset % 1f);
    }
    
    void AwardPrize(int value)
    {
        _reelAudioSource.PlayOneShot(winSound);
        DotMatrixDisplay.Message message;
        switch (value)
        {
            case 0:
                boardEvent.Invoke(new EventInfo(this, EventType.AddSaver, 25f));
                message = new DotMatrixDisplay.Message(
                    new DotMatrixDisplay.DmdAnim[]
                    {
                        new (DotMatrixDisplay.AnimType.ScrollInOut, 
                            DotMatrixDisplay.AnimOrient.Horizontal,
                            1.5f, 0, "BallSaver", TextureWrapMode.Clamp)
                    }
                );
                boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
                break;

            case 1:
                boardEvent.Invoke(new EventInfo(this, EventType.SpawnBall, null));
                message = new DotMatrixDisplay.Message(
                    new DotMatrixDisplay.DmdAnim[]
                    {
                        new (DotMatrixDisplay.AnimType.ScrollInOut, 
                            DotMatrixDisplay.AnimOrient.Horizontal,
                            1.5f, 0, "ExtraBall", TextureWrapMode.Clamp)
                    }
                );
                boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
                break;
            
            case 2:
                boardEvent.Invoke(new EventInfo(this, EventType.AddSeconds, 15));
                message = new DotMatrixDisplay.Message(
                    new DotMatrixDisplay.DmdAnim[]
                    {
                        new (DotMatrixDisplay.AnimType.ScrollInOut, 
                            DotMatrixDisplay.AnimOrient.Horizontal,
                            1.5f, 0, "ExtraTime", TextureWrapMode.Clamp)
                    }
                );
                boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
                break;

            
            case 3:
                boardEvent.Invoke(new EventInfo(this, EventType.Trigger, null));
                message = new DotMatrixDisplay.Message(
                    new DotMatrixDisplay.DmdAnim[]
                    {
                        new (DotMatrixDisplay.AnimType.ScrollInOut, 
                            DotMatrixDisplay.AnimOrient.Horizontal,
                            1.5f, 0, "AllCoins", TextureWrapMode.Clamp)
                    }
                );
                boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
                break;
        }
    }
}
