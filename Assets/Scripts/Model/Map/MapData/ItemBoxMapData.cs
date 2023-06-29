using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemBoxMapData : DirMapData
{
    public Dictionary<Pos, ItemType> itemType { get; protected set; }

    private ItemType[] fixedItemTypes;
    private ItemType[] randomItemTypes;
    private ItemType RandomItemType => randomItemTypes[Random.Range(0, randomItemTypes.Length)];

    // Create new data
    public ItemBoxMapData(StairsMapData data, int floor) : base(data)
    {
        var itemTypesSource = ResourceLoader.Instance.itemTypesData.Param(floor - 1);
        fixedItemTypes = itemTypesSource.fixedTypes;
        randomItemTypes = itemTypesSource.randomTypes;

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
            itemType[pos] = RandomItemType;
        });
    }

    // Create from custom map data
    public ItemBoxMapData(IDirMapData data, CustomMapData custom) : base(data)
    {
        var itemTypesSource = ResourceLoader.Instance.itemTypesData.Param(custom.floor - 1);
        fixedItemTypes = itemTypesSource.fixedTypes;
        randomItemTypes = itemTypesSource.randomTypes;

        itemType = new Dictionary<Pos, ItemType>();

        var boxItemPos = new Dictionary<Pos, IDirection>(custom.boxItemPos);

        int count;
        for (count = 0; count < fixedItemTypes.Length && boxItemPos.Count > 0; ++count)
        {
            Pos pos = boxItemPos.Last().Key;
            matrix[pos.x, pos.y] = Terrain.Box;
            dirMap[pos.x, pos.y] = boxItemPos[pos].Enum;
            itemType[pos] = fixedItemTypes[count];

            boxItemPos.Remove(pos);
        }

        var randomItemPos = custom.randomItemPos.ToStack();

        // Add remaining box item pos to random item pos if all of fixed items are placed. 
        boxItemPos.Keys.ForEach(remaining => randomItemPos.Push(remaining));

        // Use random item pos for box items if box item pos is not enough for fixed items.
        int surplus = fixedItemTypes.Length - count;
        for (int i = 0; i < surplus && randomItemPos.Count > 0; ++i)
        {
            Pos pos = randomItemPos.Pop();
            matrix[pos.x, pos.y] = Terrain.Box;
            dirMap[pos.x, pos.y] = rawMapData.GetValidDir(pos.x, pos.y);
            itemType[pos] = fixedItemTypes[count + i];
        }

        randomItemPos.ForEach(pos =>
        {
            itemType[pos] = RandomItemType;
        });
    }
}
