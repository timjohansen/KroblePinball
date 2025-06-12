using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Shop : EventSender
{
    RectTransform _mainTransform;
    public Image shopKeeper;
    private RectTransform _shopKeeperTransform;
    public Image selector;
    private RectTransform _selectorTransform;
    public TMP_Text speechBubbleText;

    private float _firstSlotXPos = -200f;
    private float _slotSpacing = 150f;
    
    
    private ShopState _state = ShopState.Closed;
    private float _inYPos = 0f;
    private float _outYPos = -415f;

    private float _shopKeeperOutXPos = 700f;
    private float _shopKeeperInXPos = 250f;

    private float _animTimer;
    private float _animSpeed = 3f;

    private int _selectedSlot = 0;
    // private int _slotCount = 3;
    public ShopSlot[] slots;
    private bool[] _slotSold;

    private ShopItem[] _commonItems;
    private ShopItem[] _uncommonItems;
    
    public ShopSpeechBubble speechBubble;
    
    string[] _shopOpenLines = new string[]
    {
        "Caw!",
        "Caw caw caw!",
        "Buy something!",
        "These items definitely aren't stolen!",
        "Black is the new black.",
        "Don't forget to nudge!",
        "Move your car!"
    };

    private string[] _shopSoldLines = new string[]
    {
        "Sold!",
        "Thanks!",
        "No refunds!"
    };
    
    protected override void Awake()
    {
        base.Awake();
        _mainTransform = GetComponent<RectTransform>();
        _shopKeeperTransform = shopKeeper.GetComponent<RectTransform>();
        _selectorTransform = selector.GetComponent<RectTransform>();
        _state = ShopState.Closed;
        _mainTransform.anchoredPosition = new Vector2(_mainTransform.anchoredPosition.x, _outYPos);
        _shopKeeperTransform.anchoredPosition = new Vector2(_shopKeeperOutXPos, _shopKeeperTransform.anchoredPosition.y);
    }
    
    void Start()
    {
        _commonItems = Resources.LoadAll<ShopItem>("_ShopItems/Common");
        _uncommonItems = Resources.LoadAll<ShopItem>("_ShopItems/Uncommon");
        _slotSold = new bool[slots.Length];
    }

    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            Toggle();
        }
        
        float mainYPos;
        float shopkeepXPos;
        
        switch (_state)
        {
            case ShopState.Open:
                if (GM.inst.inputMan.leftFlipperAction.action.WasPressedThisFrame())
                {
                    if (_selectedSlot == 0)
                    {
                        _selectedSlot = slots.Length - 1;
                    }
                    else
                    {
                        _selectedSlot--;
                    }
                    selector.rectTransform.anchoredPosition = new Vector2(_firstSlotXPos + _slotSpacing * _selectedSlot, selector.rectTransform.anchoredPosition.y);
                }
                else if (GM.inst.inputMan.rightFlipperAction.action.WasPressedThisFrame())
                {
                    _selectedSlot = (_selectedSlot + 1) % 3;
                    selector.rectTransform.anchoredPosition = new Vector2(_firstSlotXPos + _slotSpacing * _selectedSlot, selector.rectTransform.anchoredPosition.y);
                }
                else if (Keyboard.current.spaceKey.wasPressedThisFrame) // TODO: make this work on other devices
                {
                    if (_slotSold[_selectedSlot])
                    {
                        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "error"));
                    }
                    else if (GM.inst.coinCount < slots[_selectedSlot].itemData.price)
                    {
                        speechBubble.SetNewText("You can't afford that!");
                        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "error"));
                    }
                    else
                    {
                        Purchase(_selectedSlot);
                    }
                }
                else if (Keyboard.current.qKey.wasPressedThisFrame)
                    Close();
                
                break;
            case ShopState.Closed:
                break;
            case ShopState.AnimatingIn:
                _animTimer += Time.deltaTime * _animSpeed;
                if (_animTimer >= 1f)
                {
                    _animTimer = 1f;
                    _state = ShopState.Open;
                    
            
                    speechBubble.SetNewText(_shopOpenLines[Random.Range(0, _shopOpenLines.Length)]);
                }

                mainYPos = Mathf.Lerp(_outYPos, _inYPos, _animTimer);
                _mainTransform.anchoredPosition = new Vector2(_mainTransform.anchoredPosition.x, mainYPos);
                shopkeepXPos = Mathf.Lerp(_shopKeeperOutXPos, _shopKeeperInXPos, _animTimer);
                _shopKeeperTransform.anchoredPosition = new Vector2(shopkeepXPos, _shopKeeperTransform.anchoredPosition.y);
                break;
            
            case ShopState.AnimatingOut:
                _animTimer += Time.deltaTime * _animSpeed;
                if (_animTimer >= 1f)
                {
                    _animTimer = 1f;
                    _state = ShopState.Closed;
                }
                mainYPos = Mathf.Lerp(_inYPos, _outYPos, _animTimer);
                _mainTransform.anchoredPosition = new Vector2(_mainTransform.anchoredPosition.x, mainYPos);
                shopkeepXPos = Mathf.Lerp(_shopKeeperInXPos, _shopKeeperInXPos, _animTimer);
                _shopKeeperTransform.anchoredPosition = new Vector2(shopkeepXPos, _shopKeeperTransform.anchoredPosition.y);
                
                break;
        }
    }

    void RefreshShop()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            _slotSold[i] = false;
            slots[i].SetSold(false);
        }

        foreach (var slot in slots)
        {
            ShopItem item = null;
            int rarity = Random.Range(0, 5);
            switch (rarity)
            {
                case <= 2:
                    item = _commonItems[Random.Range(0, _commonItems.Length)];
                    break;
                case 3:
                case 4:
                    item = _uncommonItems[Random.Range(0, _uncommonItems.Length)];
                    break;
                case 5:
                    // TODO: rare
                    break;
            }
            slot.SetItem(item);
        }
    }
    
    void Open()
    {
        RefreshShop();
        if (_state == ShopState.Closed)
        {
            GM.inst.SetGameMode(GM.GameMode.Shop);
            _state = ShopState.AnimatingIn;
            _animTimer = 0;
            _selectedSlot = 0;
            selector.rectTransform.anchoredPosition = new Vector2(_firstSlotXPos, selector.rectTransform.anchoredPosition.y);
        }
    }

    void Close()
    {
        if (_state == ShopState.Open)
        {
            GM.inst.SetGameMode(GM.GameMode.Play);
            _state = ShopState.AnimatingOut;
            _animTimer = 0;
        }
    }

    void Toggle()
    {
        if (_state == ShopState.Open)
            Close();
        else if (_state == ShopState.Closed)
        {
            Open();
        }
    }

    void Purchase(int slot)
    {
        ShopItem itemData = slots[slot].itemData;
        GM.inst.ApplyItemEffect(itemData.effect, itemData.effectValue);
        slots[slot].SetSold(true);
        GM.inst.coinCount -= slots[slot].itemData.price;
        boardEvent.Invoke(new EventInfo(this, EventType.PlaySoundNoReverb, "purchase"));
        speechBubble.SetNewText(_shopSoldLines[Random.Range(0, _shopSoldLines.Length)]);
    }
    

    enum ShopState
    {
        Closed, Open, AnimatingIn, AnimatingOut
    }
    public enum ItemEffect
    {
        UpgradeChutes, UpgradeBumpers, UpgradeDropTargets, UpgradeSpinners, IncreaseCoinSpawn, AddPermanentMult, AddPermanentTime, 
        ReduceMultDecay, IncreaseNudgeStrength, DecreaseNudgeCooldown, CoinsGivePoints, DisableLetterToggle, 
        MagneticCoins, IncreaseSlotWinrate
    }
}
