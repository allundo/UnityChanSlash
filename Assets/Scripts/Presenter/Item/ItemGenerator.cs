using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemGenerator : MobGenerator<Item>
{
    private ItemData itemData;

    // FIXME: Use only the number of floors for now
    private EnemyTypesData enemyTypesData = default;

    private Dictionary<ItemType, ItemInfo> itemInfo = new Dictionary<ItemType, ItemInfo>();

    private Stack<RespawnData>[] respawnData;

    private WorldMap map;

    private ItemSource Param(ItemType type) => itemData.Param((int)type);

    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
        map = GameManager.Instance.worldMap;

        itemData = Resources.Load<ItemData>("DataAssets/Item/ItemData");
        enemyTypesData = Resources.Load<EnemyTypesData>("DataAssets/Map/EnemyTypesData");


        respawnData = new Stack<RespawnData>[enemyTypesData.Length].Select(_ => new Stack<RespawnData>()).ToArray();
    }

    void Start()
    {
        itemInfo = new ItemInfoLoader(itemData).LoadItemInfo();
        PlaceItems(map);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        var store = respawnData[this.map.floor - 1];
        var restore = respawnData[map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            for (Item item = tile.PickItem(); item != null; item = tile.PickItem())
            {
                store.Push(new RespawnData(item.itemInfo, pos));
            }
        });

        this.map = map;
        PlaceItems(map);

        while (restore.Count > 0)
        {
            Respawn(restore.Pop());
        }
    }

    private void PlaceItems(WorldMap map)
    {
        map.deadEndPos.ForEach(kvp => Put(ItemType.Potion, kvp.Key, kvp.Value.Backward));
        map.deadEndPos.Clear();
    }

    private Item Spawn(ItemInfo itemInfo, Pos pos, IDirection dir = null)
    {
        if (itemInfo == null) return null;

        dir = dir ?? Direction.north;
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

    public void Turn(IDirection dir)
    {
        transform.ForEach(tf => tf.GetComponent<Item>().SetDir(dir));
    }
    private bool Respawn(RespawnData data) => Put(data.info, data.pos);

    private struct RespawnData
    {
        public RespawnData(ItemInfo info, Pos pos)
        {
            this.info = info;
            this.pos = pos;
        }

        public ItemInfo info;
        public Pos pos;
    }
}
