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

        var fixedPos = custom.fixedItemPos.Keys.ToList();
        var fixedItemTypes = itemTypesSource.fixedTypes;

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor)
        {
            fixedItemTypes = new ItemType[2] { ItemType.KeyBlade, ItemType.IceRing };
        }
#endif

        int numOfFixed = fixedItemTypes.Length;
        int count;
        for (count = 0; count < numOfFixed && count < fixedPos.Count; ++count)
        {
            Pos pos = fixedPos[count];

            // Place Box on fixed(important) item positions.
            matrix[pos.x, pos.y] = Terrain.Box;

            dirMap[pos.x, pos.y] = custom.fixedItemPos[pos].Enum;
            itemType[pos] = fixedItemTypes[count];
        }

        var randomPos = custom.randomItemPos;

        // Add box item pos to random item pos if all of fixed item types are placed.
        for (int i = 0; i < fixedPos.Count - count; ++i)
        {
            randomPos.Add(fixedPos[count + i]);
        }

        var randomPosStack = randomPos.Except(itemType.Keys).Shuffle().ToStack();

        // Use random item pos if custom item box pos is not enough for fixed item types.
        for (int i = 0; i < numOfFixed - count && randomPosStack.Count > 0; ++i)
        {
            Pos pos = randomPosStack.Pop();

            // Place Box on fixed(important) item positions.
            matrix[pos.x, pos.y] = Terrain.Box;

            dirMap[pos.x, pos.y] = rawMapData.GetValidDir(pos.x, pos.y);
            itemType[pos] = fixedItemTypes[count + i];
        }

        var randomItemTypes = itemTypesSource.randomTypes;
        randomPosStack.ForEach(pos => itemType[pos] = randomItemTypes[UnityEngine.Random.Range(0, randomItemTypes.Length)]);
    }
}
