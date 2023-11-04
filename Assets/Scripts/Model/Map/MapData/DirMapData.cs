public interface IDirMapData
{
    Terrain[,] matrix { get; }
    Dir[,] dirMap { get; }
    RawMapData rawMapData { get; }
}

public class DirMapData : DirHandler<Terrain>, IDirMapData
{
    public Dir[,] dirMap { get; protected set; }
    public RawMapData rawMapData { get; protected set; }

    protected DirMapData(IDirMapData data) : base(data.matrix, data.rawMapData.width, data.rawMapData.height)
    {
        rawMapData = data.rawMapData;
        dirMap = data.dirMap;
    }

    // Create from custom map data
    public DirMapData(CustomMapData data) : base(data.matrix, data.width, data.height)
    {
        rawMapData = data.rawMapData;
        dirMap = CreateDirMap(width, height);
        data.customStructurePos.ForEach(kv => dirMap[kv.Key.x, kv.Key.y] = kv.Value.Enum);
    }

    // Create new map
    public DirMapData(int[,] rawMapMatrix, int width, int height) : base(null, width, height)
    {
        rawMapData = new RawMapData(rawMapMatrix, width, height);
        matrix = new Terrain[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                matrix[i, j] = Util.ConvertTo<Terrain>(rawMapMatrix[i, j]);
            }
        }

        dirMap = CreateDirMap(width, height);
    }

    // Import from save data.
    public DirMapData(DataStoreAgent.MapData mapData) : base(null, mapData.mapSize, mapData.mapMatrix.Length / mapData.mapSize)
    {
        rawMapData = RawMapData.Convert(mapData.mapMatrix, width);

        matrix = new Terrain[width, height];
        dirMap = new Dir[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                matrix[i, j] = Util.ConvertTo<Terrain>(mapData.mapMatrix[i + j * width]);
                dirMap[i, j] = Util.ConvertTo<Dir>(mapData.dirMap[i + j * width]);
            }
        }
    }

    private Dir[,] CreateDirMap(int width, int height)
    {
        Dir[,] ret = new Dir[width, height];

        // Set direction to door and wall and convert some walls to pillar
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ret[x, y] = CreateDir(x, y, matrix[x, y]);
            }
        }

        return ret;
    }

    protected Dir CreateDir(int x, int y, Terrain terrain)
        => IsGridPoint(x, y) ? GetGridPointDir(x, y, terrain) : GetNonGridPointDir(x, y, terrain);

    private bool IsGridPoint(int x, int y) => x % 2 == 0 && y % 2 == 0;

    private Dir GetGridPointDir(int x, int y, Terrain terrain)
    {
        if (terrain == Terrain.MessagePillar || terrain == Terrain.BloodMessagePillar || terrain == Terrain.Box)
        {
            return rawMapData.GetValidDir(x, y);
        }

        Dir doorDir = rawMapData.GetDoorDir(x, y);

        // Leave it as is if strait wall or room ground
        if (doorDir == Dir.NONE)
        {
            Dir wallDir = rawMapData.GetWallDir(x, y);
            if (wallDir == Dir.NS || wallDir == Dir.EW || wallDir == Dir.NONE)
            {
                return wallDir;
            }
        }

        // Set gate as wall next to door
        // Set pillar as corner wall
        matrix[x, y] = Terrain.Pillar;

        return doorDir;
    }

    private Dir GetNonGridPointDir(int x, int y, Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.Door:
                return rawMapData.GetGateDir(x, y);

            case Terrain.LockedDoor:
                return rawMapData.GetPathDir(x, y);

            case Terrain.MessageWall:
            case Terrain.BloodMessageWall:
            case Terrain.SealableDoor:
            case Terrain.ExitDoor:
            case Terrain.Box:
            case Terrain.Fountain:
                return rawMapData.GetValidDir(x, y);

            case Terrain.UpStairs:
            case Terrain.DownStairs:
                return rawMapData.GetNotWallDir(x, y);
        }

        return rawMapData.GetWallDir(x, y);
    }

}
