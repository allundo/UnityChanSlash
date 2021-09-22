using UnityEngine;
using System.Collections.Generic;

public class ItemGenerator : Generator<Item>
{
    [SerializeField] private Material potion = default;
    private Dictionary<ItemType, Material> itemMaterials = new Dictionary<ItemType, Material>();

    private WorldMap map;

    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
        map = GameManager.Instance.worldMap;

        itemMaterials[ItemType.Potion] = potion;
    }

    void Start()
    {
        map.deadEndPos.ForEach(kvp => Put(ItemType.Potion, kvp.Key, kvp.Value.Backward));
    }

    private Item Spawn(ItemInfo itemInfo, Pos pos, IDirection dir = null)
    {
        return base.Spawn(map.WorldPos(pos), dir)
            .SetItemInfo(itemInfo)
            .SetMaterial(itemMaterials[itemInfo.type]);
    }

    public Item Put(ItemInfo itemInfo, Pos pos, IDirection dir = null)
    {
        var item = Spawn(itemInfo, pos, dir);

        map.GetTile(pos).PutItem(item);

        return item;
    }

    public Item Put(ItemType itemType, Pos pos, IDirection dir = null)
        => Put(new ItemInfo(itemType), pos, dir);

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
