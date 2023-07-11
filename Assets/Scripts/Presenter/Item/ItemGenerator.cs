using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemGenerator : MobGenerator<Item>
{
    private Stack<RespawnData>[] respawnData;

    private WorldMap map;

    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;

        respawnData = new Stack<RespawnData>[GameInfo.Instance.LastFloor].Select(_ => new Stack<RespawnData>()).ToArray();
    }

    public void SwitchWorldMap(WorldMap map)
    {
        respawnData[this.map.floor - 1] = StoreRespawnData(true);

        if (!PlaceItems(map)) StackRespawn(respawnData[map.floor - 1]);
    }

    private Stack<RespawnData> StoreRespawnData(bool clearItems = false)
    {
        var store = new Stack<RespawnData>();

        this.map.ForEachTiles((tile, pos) =>
        {
            for (Item item = tile.PickItem(); item != null; item = tile.PickItem())
            {
                store.Push(new RespawnData(item.itemInfo, pos));
            }
        });

        if (!clearItems) StackRespawn((store.ToArray().Clone() as RespawnData[]).ToStack());

        return store;
    }

    private void StackRespawn(Stack<RespawnData> restore)
    {
        var playerDir = PlayerInfo.Instance.Dir;
        while (restore.Count > 0)
        {
            Respawn(restore.Pop(), playerDir);
        }
    }

    public bool PlaceItems(WorldMap map)
    {
        this.map = map;

        ItemBoxMapData itemData = map.itemBoxMapData;
        if (itemData == null || itemData.itemType.Count == 0) return false;

        var playerDir = PlayerInfo.Instance.Dir;
        itemData.itemType.ForEach(kv => PutNew(kv.Value, kv.Key, playerDir));
        itemData.itemType.Clear();
        return true;
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

    public bool PutNew(ItemType itemType, Pos pos, IDirection dir = null)
        => Put(ResourceLoader.Instance.ItemInfo(itemType), pos, dir);

    public Item PlaceItem(ItemType itemType, int numOfItem, Pos pos, IDirection dir = null)
    {
        var item = Spawn(ResourceLoader.Instance.ItemInfo(itemType, numOfItem), pos, dir);
        if (map.GetTile(pos).PutItem(item)) return item;

        item.Inactivate();
        return null;
    }

    private bool Respawn(RespawnData data, IDirection dir = null) => Put(data.info, data.pos, dir);

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

    public List<DataStoreAgent.ItemData>[] ExportRespawnData()
    {
        var export = Convert(respawnData);
        export[this.map.floor - 1] = StoreRespawnData().Select(data => Convert(data)).ToList();
        return export;
    }

    private DataStoreAgent.ItemData Convert(RespawnData data)
        => new DataStoreAgent.ItemData(data.pos, data.info.type, data.info.numOfItem);

    private List<DataStoreAgent.ItemData>[] Convert(Stack<RespawnData>[] dataSet)
        => dataSet.Select(dataList => dataList.Select(data => Convert(data)).ToList()).ToArray();

    public void ImportRespawnData(List<DataStoreAgent.ItemData>[] import, WorldMap currentMap)
    {
        respawnData = Convert(import);
        this.map = currentMap;
        StackRespawn(respawnData[currentMap.floor - 1]);
    }

    private RespawnData Convert(DataStoreAgent.ItemData data)
        => new RespawnData(ResourceLoader.Instance.ItemInfo(data.itemType, data.numOfItem), data.pos);

    private Stack<RespawnData>[] Convert(List<DataStoreAgent.ItemData>[] dataSet)
    {
        return dataSet.Select(dataList =>
         {
             return dataList.Select(data =>
             {
                 return Convert(data);
             }).ToStack();
         }).ToArray();
    }
}
