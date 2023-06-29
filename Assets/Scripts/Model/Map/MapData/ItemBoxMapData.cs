using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemBoxMapData : DirMapData
{
    public Dictionary<Pos, ItemType> itemType { get; protected set; }

    // Create new data
    public ItemBoxMapData(StairsMapData data, int floor) : base(data)
    {
        var itemTypesSource = ResourceLoader.Instance.itemTypesData.Param(floor - 1);
        var fixedItemTypes = itemTypesSource.fixedTypes;
        var randomItemTypes = itemTypesSource.randomTypes;

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor)
        {
            fixedItemTypes = new ItemType[1] { ItemType.KeyBlade };
        }
#endif

        itemType = new Dictionary<Pos, ItemType>();

        var itemPos = new Dictionary<Pos, IDirection>(data.deadEndPos);
        if (!data.upStairs.IsNull) itemPos.Remove(data.upStairs);
        if (!data.downStairs.IsNull) itemPos.Remove(data.downStairs);
        if (!data.exitDoor.IsNull) itemPos.Remove(data.StairsBottom);

        for (int i = 0; i < fixedItemTypes.Length && itemPos.Count > 0; ++i)
        {
            Pos pos = itemPos.Last().Key;
            matrix[pos.x, pos.y] = Terrain.Box;
            dirMap[pos.x, pos.y] = itemPos[pos].Enum;
            itemType[pos] = fixedItemTypes[i];

            itemPos.Remove(pos);
        }

        itemPos.Keys.ForEach(pos =>
        {
            itemType[pos] = randomItemTypes[Random.Range(0, randomItemTypes.Length)];
        });
    }

    // Create from custom map data
    public ItemBoxMapData(IDirMapData data, CustomMapData custom) : base(data)
    {
        var itemTypesSource = ResourceLoader.Instance.itemTypesData.Param(custom.floor - 1);

        var customItem = new CustomItemPos(data, itemTypesSource.fixedTypes, itemTypesSource.randomTypes, Terrain.Box);
        customItem.SetCustomDataPos(custom.boxItemPos, custom.randomItemPos);
        this.itemType = customItem.itemType;
    }
}
