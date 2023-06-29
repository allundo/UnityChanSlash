using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemGenerator : MobGenerator<Item>
{
    private ItemData itemData;
    private ItemTypesData itemTypesData;

    private ItemType[] singleItemTypes;
    private ItemType[] randomItemTypes;
    private ItemType RandomItemType => randomItemTypes[Random.Range(0, randomItemTypes.Length)];

    private Stack<RespawnData>[] respawnData;

    private WorldMap map;

    private ItemSource Param(ItemType type) => itemData.Param((int)type);

    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;

        itemData = ResourceLoader.Instance.itemData;
        itemTypesData = ResourceLoader.Instance.itemTypesData;

        respawnData = new Stack<RespawnData>[GameInfo.Instance.LastFloor].Select(_ => new Stack<RespawnData>()).ToArray();
    }

    void Start()
    {
        SetWorldMap(GameManager.Instance.worldMap);
        PlaceItems(map);
    }

    private void SetWorldMap(WorldMap map)
    {
        this.map = map;
        randomItemTypes = itemTypesData.Param(map.floor - 1).randomTypes;
        singleItemTypes = itemTypesData.Param(map.floor - 1).fixedTypes;

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor)
        {
            singleItemTypes = new ItemType[1] { ItemType.KeyBlade };
        }
#endif
    }

    public void SwitchWorldMap(WorldMap map)
    {
        respawnData[this.map.floor - 1] = GetRespawnData(true);
        RespawnItems(map);
    }

    private Stack<RespawnData> GetRespawnData(bool clearItems = false)
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

    private void RespawnItems(WorldMap respawnMap)
    {
        // Switch current world map.
        SetWorldMap(respawnMap);
        PlaceItems(respawnMap);

        StackRespawn(respawnData[respawnMap.floor - 1]);
    }

    private void StackRespawn(Stack<RespawnData> restore)
    {
        var dir = PlayerInfo.Instance.Dir;
        while (restore.Count > 0)
        {
            Respawn(restore.Pop(), dir);
        }
    }

    private void PlaceItems(WorldMap map)
    {
        IDirection playerDir = PlayerInfo.Instance.Dir;

        if (map.itemBoxMapData != null)
        {
            map.itemBoxMapData.itemType.ForEach(kv => PutNew(kv.Value, kv.Key, playerDir));
            map.itemBoxMapData.itemType.Clear();
        }
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
        export[this.map.floor - 1] = GetRespawnData().Select(data => Convert(data)).ToList();
        return export;
    }

    private DataStoreAgent.ItemData Convert(RespawnData data)
        => new DataStoreAgent.ItemData(data.pos, data.info.type, data.info.numOfItem);

    private List<DataStoreAgent.ItemData>[] Convert(Stack<RespawnData>[] dataSet)
        => dataSet.Select(dataList => dataList.Select(data => Convert(data)).ToList()).ToArray();

    public void ImportRespawnData(List<DataStoreAgent.ItemData>[] import, WorldMap currentMap)
    {
        respawnData = Convert(import);
        RespawnItems(currentMap);
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
