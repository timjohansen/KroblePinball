using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopSlot : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text priceText;
    public Image icon;
    public GameObject soldSign;
    public ShopItem itemData;
    
    
    public void SetItem(ShopItem item)
    {
        itemData = item;
        nameText.text = item.displayName;
        descText.text = item.description;
        priceText.text = item.price.ToString();
        icon.sprite = item.image;
    }

    public void SetSold(bool sold)
    {
        soldSign.SetActive(sold);
    }
}
