using System;
using System.Linq;
using System.Collections.Generic;

public class CustomItemPos : CustomPosHandler<ItemType>
{
    public Dictionary<Pos, ItemType> itemType => storedData;
    private ItemType[] fixedData;

    public CustomItemPos(IDirMapData data, ItemType[] fixedData, ItemType[] randomData, Terrain type)
        : base(data, fixedData.Length, type, () => randomData[UnityEngine.Random.Range(0, randomData.Length)])
    {
        this.fixedData = fixedData;
    }

    protected override void SetFixedData(Pos pos, ItemType data) => SetRandomData(pos, data); // Set ItemType to storeData
    protected override ItemType GetFixedData(int index) => fixedData[index];
}

public class CustomMessagePos : CustomPosHandler<int>
{
    public List<Pos> fixedMessagePos { get; protected set; } = new List<Pos>();
    public Dictionary<Pos, int> randomMessagePos => storedData;

    public CustomMessagePos(IDirMapData data, int numOfFixed, Terrain type, Func<int> getRandomData)
        : base(data, numOfFixed, type, getRandomData) { }

    protected override void SetFixedData(Pos pos, int data) => fixedMessagePos.Add(pos);
    protected override int GetFixedData(int index) => 0; // Never used
}

public abstract class CustomPosHandler<T> : DirMapData
{
    protected Dictionary<Pos, T> storedData = new Dictionary<Pos, T>();

    protected Terrain type;
    protected int numOfFixed;
    protected Func<T> getRandomData;

    public CustomPosHandler(IDirMapData data, int numOfFixed, Terrain type, Func<T> getRandomData) : base(data)
    {
        this.type = type;
        this.numOfFixed = numOfFixed;
        this.getRandomData = getRandomData;
    }

    public void SetCustomDataPos(Dictionary<Pos, IDirection> fixedCustomPos, List<Pos> randomCustomPos)
    {
        var fixedPos = fixedCustomPos.Keys.ToList();

        int count;
        for (count = 0; count < numOfFixed && count < fixedPos.Count; ++count)
        {
            Pos pos = fixedPos[count];

            matrix[pos.x, pos.y] = type;
            dirMap[pos.x, pos.y] = fixedCustomPos[pos].Enum;
            SetFixedData(pos, GetFixedData(count));
        }

        var randomPos = randomCustomPos.ToStack();

        // Add fixed message pos to random message pos if all of fixed message are placed. 
        for (int i = 0; i < fixedPos.Count - count; ++i)
        {
            randomPos.Push(fixedPos[count + i]);
        }

        // Use random message pos if custom fixed pos is not enough for fixed messages.
        for (int i = 0; i < numOfFixed - count; ++i)
        {
            Pos pos = randomPos.Pop();
            dirMap[pos.x, pos.y] = rawMapData.GetValidDir(pos.x, pos.y);
            SetFixedData(pos, GetFixedData(count + 1));
        }

        randomPos.ForEach(pos => SetRandomData(pos, getRandomData()));
    }

    protected abstract void SetFixedData(Pos pos, T data);
    protected abstract T GetFixedData(int index);
    protected void SetRandomData(Pos pos, T data) => storedData[pos] = data;
}
