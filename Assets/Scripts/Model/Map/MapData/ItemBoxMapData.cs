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
            // Place random item by 60% probability
            if (Util.DiceRoll(3, 5)) itemType[pos] = randomItemTypes[Random.Range(0, randomItemTypes.Length)];
        });
    }

    // Create from custom map data
    public ItemBoxMapData(IDirMapData data, CustomMapData custom) : base(data)
    {
        itemType = new Dictionary<Pos, ItemType>();

        var itemTypesSource = ResourceLoader.Instance.itemTypesData.Param(custom.floor - 1);

        var fixedPos = custom.boxItemPos.Keys.ToList();
        var fixedItemTypes = itemTypesSource.fixedTypes;
        int numOfFixed = fixedItemTypes.Length;

        int count;
        for (count = 0; count < numOfFixed && count < fixedPos.Count; ++count)
        {
            Pos pos = fixedPos[count];

            matrix[pos.x, pos.y] = Terrain.Box;
            dirMap[pos.x, pos.y] = custom.boxItemPos[pos].Enum;
            itemType[pos] = fixedItemTypes[count];
        }

        var randomPos = custom.randomItemPos;

        // Add fixed message pos to random message pos if all of fixed message are placed. 
        for (int i = 0; i < fixedPos.Count - count; ++i)
        {
            randomPos.Add(fixedPos[count + i]);
        }

        var randomPosStack = randomPos.Shuffle().ToStack();

        // Use random message pos if custom fixed pos is not enough for fixed messages.
        for (int i = 0; i < numOfFixed - count && randomPosStack.Count > 0; ++i)
        {
            Pos pos = randomPosStack.Pop();
            dirMap[pos.x, pos.y] = rawMapData.GetValidDir(pos.x, pos.y);
            itemType[pos] = fixedItemTypes[count + 1];
        }

        var randomItemTypes = itemTypesSource.randomTypes;
        randomPosStack.ForEach(pos => itemType[pos] = randomItemTypes[UnityEngine.Random.Range(0, randomItemTypes.Length)]);
    }
}
