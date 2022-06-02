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

    private Dictionary<ItemType, ItemInfo> itemInfo;

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
        itemInfo = new ItemInfoLoader(itemData).LoadItemInfo();

        SetWorldMap(GameManager.Instance.worldMap);
        PlaceItems(map);
    }

    private void SetWorldMap(WorldMap map)
    {
        this.map = map;
        randomItemTypes = itemTypesData.Param(map.floor - 1).randomTypes;
        singleItemTypes = itemTypesData.Param(map.floor - 1).singleTypes;

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor)
        {
            singleItemTypes = new ItemType[1] { ItemType.KeyBlade };
        }
#endif
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

        SetWorldMap(map);
        PlaceItems(map);

        while (restore.Count > 0)
        {
            Respawn(restore.Pop());
        }
    }

    private void PlaceItems(WorldMap map)
    {
        // TODO: Need to implement item placing process
        if (map.deadEndPos.Count == 0) return;

        for (int i = 0; i < singleItemTypes.Length && map.deadEndPos.Count > 0; i++)
        {
            var last = map.deadEndPos.Last();
            Put(singleItemTypes[i], last.Key, last.Value.Backward);
            map.deadEndPos.Remove(last.Key);
        }

        map.deadEndPos.ForEach(kvp => Put(RandomItemType, kvp.Key, kvp.Value.Backward));
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
