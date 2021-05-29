using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OwnedItemsData
{
    private const string PlayerPrefsKey = "OWNED_ITEMS_DATA";

    private static OwnedItemsData _instance = null;
    public static OwnedItemsData Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = PlayerPrefs.HasKey(PlayerPrefsKey)
                ? JsonUtility.FromJson<OwnedItemsData>(PlayerPrefs.GetString(PlayerPrefsKey))
                : new OwnedItemsData();
            }

            return _instance;
        }
    }

    public OwnedItem[] OwnedItems
    {
        get { return ownedItems.ToArray(); }
    }

    [SerializeField] private List<OwnedItem> ownedItems = new List<OwnedItem>();

    private OwnedItemsData()
    {

    }

    // Start is called before the first frame update
    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    public void Add(Item.ItemTypeEnum type, int amount = 1)
    {
        OwnedItem item = GetItem(type);
        if (null == item)
        {
            item = new OwnedItem(type);
            ownedItems.Add(item);
        }
        item.Add(amount);
    }
    public void Use(Item.ItemTypeEnum type, int amount = 1)
    {
        OwnedItem item = GetItem(type);
        if (null == item || item.Number < amount)
        {
            throw new Exception("アイテムが足りません");
        }
        item.Use(amount);
    }

    public OwnedItem GetItem(Item.ItemTypeEnum type)
    {
        return ownedItems.FirstOrDefault(item => item.Type == type);
    }
}

[Serializable]
public class OwnedItem
{
    [SerializeField] private Item.ItemTypeEnum type;
    [SerializeField] private int number;

    public OwnedItem(Item.ItemTypeEnum type)
    {
        this.type = type;
    }
    public Item.ItemTypeEnum Type
    {
        get { return type; }
    }

    public int Number
    {
        get { return number; }
    }

    public void Add(int amount = 1)
    {
        this.number += amount;
    }

    public void Use(int amount = 1)
    {
        this.number -= amount;
    }
}