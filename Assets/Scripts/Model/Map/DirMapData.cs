using System.Collections.Generic;

public class DirMapData
{
    public Terrain[,] matrix { get; private set; }
    public Dir[,] dirMap { get; private set; }
    private RawMapData rawMapData;

    public DirMapData(int[] customMapData, Terrain[,] matrix, int width)
    {
        int height = customMapData.Length / width;

        rawMapData = RawMapData.Convert(customMapData, width);
        this.matrix = matrix;

        dirMap = CreateDirMap(width, height);
    }

    public DirMapData(int[,] rawMapMatrix, int width, int height)
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

    public DirMapData(DataStoreAgent.MapData mapData)
    {
        int width = mapData.mapSize;
        int height = mapData.mapMatrix.Length / width;

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

    public Dictionary<Pos, IDirection> SearchDeadEnds() => rawMapData.SearchDeadEnds();

    public Dir CreateDir(int x, int y, Terrain terrain)
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
            case Terrain.UpStairs:
            case Terrain.DownStairs:
            case Terrain.ExitDoor:
                return rawMapData.GetValidDir(x, y);

            case Terrain.Box:
                return rawMapData.GetNotWallDir(x, y);
        }

        return rawMapData.GetWallDir(x, y);
    }
}
