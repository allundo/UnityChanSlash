using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using UniRx;

public class GameInfo : SingletonMonoBehaviour<GameInfo>
{
    public static readonly int MAX_FLOOR = 10;

    private WorldMap[] maps;
    private int[] mapSize;

    public void SetMapComp()
    {
        float totalMapArea = 0;
        float totalMapDiscovered = 0;

        for (int i = 0; i < LastFloor; i++)
        {
            totalMapArea += mapSize[i] * mapSize[i];
            totalMapDiscovered += (maps[i] != null) ? maps[i].SumUpDiscovered() : 0;
        }

        mapComp = totalMapDiscovered / totalMapArea;
    }

    public int currentFloor = 1;

    public ulong moneyAmount = 0;
    public float mapComp = 0f;
    public int clearTimeSec = 0;
    public int defeatCount = 0;
    public int level = 1;
    public int strength = 10;
    public int magic = 10;

    public int startActionID = 0;
    public bool isDebugFloorLoaded => maps[MAX_FLOOR] != null;

    public int LastFloor => ResourceLoader.Instance.enemyTypesData.Length;

#if UNITY_EDITOR
    public bool isScenePlayedByEditor = true;
#endif

    public WorldMap Map(int floor)
    {
        if (floor == 0)
        {
#if UNITY_EDITOR
            if (isScenePlayedByEditor) currentFloor = 2;
#endif
            floor = currentFloor;

            // DEBUG ONLY
            if (Debug.isDebugBuild && isDebugFloorLoaded)
            {
                maps[1] = maps[MAX_FLOOR];
            }
        }

        if (floor > 0 && floor <= LastFloor)
        {
            int size = mapSize[floor - 1];
            return maps[floor - 1] = (maps[floor - 1] ?? new WorldMap(floor, size, size));
        }

        return null;
    }

    public WorldMap NextFloorMap(bool isDownStair = true) => Map(isDownStair ? ++currentFloor : --currentFloor);

    protected override void Awake()
    {
        base.Awake();

        DOTween.SetTweensCapacity(500, 500);
        Application.targetFrameRate = Constants.FRAME_RATE;

        InitData();

        // DEBUG ONLY
        if (Debug.isDebugBuild) CreateDebugMap();
    }

    public void CreateDebugMap()
    {
        if (isDebugFloorLoaded) return;

        int[] matrix =
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1, 1, 4, 1,12, 1, 1, 1, 1,11, 2,
            6, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           10, 0, 4, 1, 1,11, 2, 1,12, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,12, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 7, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        var deadEnds = new Dictionary<Pos, IDirection>()
        {
            { new Pos(5, 5),    Direction.west  },
            { new Pos(1, 3),    Direction.north },
            { new Pos(1, 5),    Direction.north },
            { new Pos(13, 13),  Direction.north },
            { new Pos(1, 1),    Direction.south },
        };

        maps[MAX_FLOOR] = new WorldMap(new MapManager(2, matrix, 15, deadEnds));
    }

    private int[] FinalMapMatrix()
    {
        return new[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1,11, 2,11, 1, 1, 1,11, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 0, 1, 1,11, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,11, 1, 1, 1,11, 2,11, 1, 1, 1,11, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 4, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,11, 1, 1, 1, 1, 1,11, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 4, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 4, 2, 2, 2, 2, 2, 2, 2, 0, 2, 1, 7, 1, 7, 1, 7, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,11, 1, 1, 1,11, 4, 0, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 2, 0, 2, 0, 2, 1, 7, 1, 7, 1, 7, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 2, 0, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1,11, 1, 1, 2, 0, 2, 0, 2, 1, 7, 1, 7, 1, 7, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,11, 1, 1, 1,11, 4, 0, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 4, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 4, 0, 4, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,11, 1, 1, 1,11, 2, 2, 2,
            2, 0, 2, 0, 2, 0, 2, 0, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 2, 2, 2,
            2, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,11, 1, 1, 2, 2, 2,
            2, 4, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 2, 2, 2,
            2, 1, 1, 1, 1,11, 1, 1, 1,11, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,11, 1, 1, 1,11, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1,11, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,11, 1, 1, 1,11, 1, 1, 1,11, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };
    }

    private Dictionary<Pos, IDirection> FinalMapDeadEnds()
    {
        return new Dictionary<Pos, IDirection>()
        {
            { new Pos(14, 12), Direction.north },
            { new Pos(16, 12), Direction.north },
            { new Pos(16, 14), Direction.north },
            { new Pos(16, 16), Direction.north },
            { new Pos(14, 16), Direction.north },
            { new Pos(12, 16), Direction.north },
            { new Pos(12, 14), Direction.north },
            { new Pos(12, 12), Direction.north },
            { new Pos( 3, 21), Direction.north },
            { new Pos(11, 27), Direction.north },
            { new Pos( 7, 13), Direction.south },
            { new Pos(13,  7), Direction.west  },
            { new Pos(14, 14), Direction.north }, // KeyBlade
            { new Pos( 1,  1), Direction.south }, // up stairs
        };
    }

    public void ClearMaps()
    {
        maps = Enumerable.Repeat<WorldMap>(null, MAX_FLOOR + 1).ToArray();
        mapSize = Enumerable.Repeat(49, MAX_FLOOR + 1).ToArray();
        currentFloor = 1;

        mapSize[0] = 19;
        mapSize[1] = 35;
        mapSize[LastFloor - 1] = 29;

        maps[LastFloor - 1] = new WorldMap(new MapManager(LastFloor, FinalMapMatrix(), 29, FinalMapDeadEnds()));
    }

    public void InitData(bool clearMaps = true)
    {
        if (clearMaps) ClearMaps();

        moneyAmount = 0;
        mapComp = 0f;
        clearTimeSec = 0;
        defeatCount = 0;
        level = 1;
        strength = 10;
        magic = 10;
    }

    public DataStoreAgent.MapData[] ExportMapData()
    {
        var export = Enumerable.Repeat<DataStoreAgent.MapData>(null, LastFloor).ToArray();
        for (int i = 0; i < LastFloor; i++)
        {
            if (maps[i] == null) continue;
            export[i] = new DataStoreAgent.MapData(maps[i]);
        }
        return export;
    }

    public void ImportGameData(DataStoreAgent.SaveData import)
    {
        ClearMaps();

        this.currentFloor = import.currentFloor;

        for (int i = 0; i < LastFloor; i++)
        {
            var mapData = import.mapData[i];

            if (mapData == null) continue;

            var mapSize = mapData.mapSize;

            maps[i] = new WorldMap(new MapManager(i + 1, mapData));
            maps[i].ImportMapData(mapData);
        }
    }
}
