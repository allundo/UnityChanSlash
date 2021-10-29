using UnityEngine;
using System.Collections.Generic;

public class ItemGenerator : Generator<Item>
{
    [SerializeField] private Material matPotion = default;
    private Dictionary<ItemType, ItemInfo> itemInfo = new Dictionary<ItemType, ItemInfo>();

    private WorldMap map;

    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
        map = GameManager.Instance.worldMap;

        itemInfo[ItemType.Potion] = new PotionInfo(matPotion);
    }

    void Start()
    {
        map.deadEndPos.ForEach(kvp => Put(ItemType.Potion, kvp.Key, kvp.Value.Backward));
    }

    private Item Spawn(ItemInfo itemInfo, Pos pos, IDirection dir = null)
    {
        if (itemInfo == null) return null;

        return base.Spawn(map.WorldPos(pos), dir).SetItemInfo(itemInfo);
    }

    public bool Put(ItemInfo itemInfo, Pos pos, IDirection dir = null)
    {
        var item = Spawn(itemInfo, pos, dir);

        if (map.GetTile(pos).PutItem(item)) return true;

        item.Inactivate();
        return false;
    }

    public bool Put(ItemType itemType, Pos pos, IDirection dir = null, int numOfItem = 1)
        => Put(itemInfo[itemType].Clone(numOfItem) as ItemInfo, pos, dir);

    public ItemGenerator SetPoolObject(GameObject itemPool)
    {
        pool = itemPool.transform;
        return this;
    }

    public void Turn(IDirection dir)
    {
        transform.ForEach(tf => tf.GetComponent<Item>().SetDir(dir));
    }
}
