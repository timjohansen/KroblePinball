using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/ShopItem", order = 1)]
public class ShopItem : ScriptableObject
{
    public Sprite image;
    public string displayName;
    public string description;

    public Rarity rarity;
    public int price;
    public Shop.ItemEffect effect;
    public int effectValue;

    public enum Rarity
    {
        Common, Uncommon, Rare
    }
}
