using System;
using System.Collections.Generic;

public class RawMapData : DirHandler<int>
{
    public static readonly int PATH = MazeCreator.PATH;
    public static readonly int GROUND = MazeCreator.GROUND;
    public static readonly int WALL = MazeCreator.WALL;
    public static readonly int DOOR = MazeCreator.DOOR;
    public static readonly int PIT = (int)Terrain.Pit;

    private static readonly Dictionary<int, int> RAW_DATA_MAP = new Dictionary<int, int>()
    {
        { (int)Terrain.Path,                  PATH   },
        { (int)Terrain.Ground,                GROUND },
        { (int)Terrain.Wall,                  WALL   },
        { (int)Terrain.MessageWall,           WALL   },
        { (int)Terrain.BloodMessageWall,      WALL   },
        { (int)Terrain.Pillar,                WALL   },
        { (int)Terrain.MessagePillar,         WALL   },
        { (int)Terrain.BloodMessagePillar,    WALL   },
        { (int)Terrain.Door,                  DOOR   },
        { (int)Terrain.OpenDoor,              DOOR   },
        { (int)Terrain.SealableDoor,          DOOR   },
        { (int)Terrain.LockedDoor,            DOOR   },
        { (int)Terrain.ExitDoor,              DOOR   },
        { (int)Terrain.Pit,                   PIT    },
    };

    private static int ConvertToRaw(int data) => RAW_DATA_MAP.TryGetValue(data, out data) ? data : GROUND;

    public int[,] CopyMatrix() => matrix.Clone() as int[,];

    public RawMapData(int[,] matrix, int width, int height) : base(matrix, width, height)
    { }

    public static RawMapData Convert(int[] mapData, int width)
    {
        var height = mapData.Length / width;

        int[,] matrix = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[x, y] = ConvertToRaw(mapData[x + y * width]);
            }
        }

        return new RawMapData(matrix, width, height);
    }

    private Pos[] GetPaths()
    {
        var list = new List<Pos>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (matrix[x, y] == MazeCreator.PATH) list.Add(new Pos(x, y));
            }
        }

        return list.ToArray();
    }

    public Dictionary<Pos, IDirection> SearchDeadEnds()
    {
        var deadEndPos = new Dictionary<Pos, IDirection>();

        GetPaths().ForEach(pos =>
        {
            var list = new List<IDirection>();

            if (matrix[pos.x, pos.y - 1] != WALL) list.Add(Direction.north);
            if (matrix[pos.x, pos.y + 1] != WALL) list.Add(Direction.south);
            if (matrix[pos.x - 1, pos.y] != WALL) list.Add(Direction.west);
            if (matrix[pos.x + 1, pos.y] != WALL) list.Add(Direction.east);

            if (list.Count == 1) deadEndPos.Add(pos, list[0]);
        });

        return deadEndPos;
    }

    public Dir GetDir(int x, int y, int terrainID) => GetDir(x, y, terrainID, -1);

    public Dir GetValidDir(int x, int y, Func<int, bool> validChecker)
    {
        if (y > 0 && validChecker(matrix[x, y - 1])) return Dir.N;
        if (x > 0 && validChecker(matrix[x - 1, y])) return Dir.W;
        if (y < height - 2 && validChecker(matrix[x, y + 1])) return Dir.S;
        if (x < width - 2 && validChecker(matrix[x + 1, y])) return Dir.E;

        return Dir.NONE;
    }

    public Dir GetDoorDir(int x, int y) => GetDir(x, y, DOOR);
    public Dir GetWallDir(int x, int y) => GetDir(x, y, WALL);

    public Dir GetGateDir(int x, int y)
    {
        Dir validDir = GetValidDir(x, y);
        Dir invDir = Direction.Convert(validDir).Backward.Enum;
        return Dir.NESW ^ validDir ^ invDir;
    }

    public Dir GetValidDir(int x, int y)
        => GetValidDir(x, y, terrainID => terrainID == GROUND || terrainID == PATH || terrainID == DOOR);
    public Dir GetPathDir(int x, int y)
        => GetValidDir(x, y, terrainID => terrainID == PATH);
    public Dir GetNotWallDir(int x, int y)
        => GetValidDir(x, y, terrainID => terrainID != WALL);

#if UNITY_EDITOR
    public void DebugMatrix()
    {
        var str = new System.Text.StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                str.Append(matrix[x, y]);
            }
            str.Append("\n");
        }

        UnityEngine.Debug.Log(str);
    }
#endif
}
