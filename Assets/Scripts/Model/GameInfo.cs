using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

public class GameInfo : SingletonMonoBehaviour<GameInfo>
{
    public static readonly int MAX_FLOOR = 10;

    private WorldMap[] maps;
    private int[] mapSize;

    private float CalcMapComp()
    {
        float totalMapArea = 0;
        float totalMapDiscovered = 0;

        for (int i = 0; i < LastFloor; i++)
        {
            totalMapArea += mapSize[i] * mapSize[i];
            totalMapDiscovered += (maps[i] != null) ? maps[i].SumUpDiscovered() : 0;
        }

        return totalMapArea > 0 ? totalMapDiscovered / totalMapArea : 0f;
    }

    private float CalcTreasureComp()
    {
        int sum = 0;
        int open = 0;
        for (int i = 0; i < LastFloor; i++)
        {
            if (maps[i] == null) break;
            maps[i].ForEachTiles(tile =>
            {
                if (tile is Box)
                {
                    if ((tile as Box).IsOpen) open++;
                    sum++;
                }
            });
        }
        return sum > 0 ? (float)open / (float)sum : 0f;
    }

    public int currentFloor = 1;

    public ulong moneyAmount = 0;
    public float mapComp = 0f;
    public float treasureComp = 0f;
    public int endTimeSec = 0;
    public int defeatCount = 0;
    public int level = 1;
    public int strength = 10;
    public int magic = 10;
    public int clearRank = 0;
    public string title = "なし";
    public float titleBonusRatio = 1f;
    public int steps = 0;
    public int storedTimeSec = 0;

    public DataStoreAgent.ClearRecord clearRecord = null;

    public void TotalClearCounts(PlayerCounter counter, int clearTimeSec, ulong moneyAmount)
    {
        this.endTimeSec = clearTimeSec;
        this.moneyAmount = moneyAmount;
        this.mapComp = CalcMapComp();
        this.treasureComp = CalcTreasureComp();

        var clearData = counter.TotalClearCounts(mapComp, treasureComp, clearTimeSec);
        title = clearData.title;
        titleBonusRatio = clearData.bonusRate;
        defeatCount = clearData.defeatCount;

        this.steps = counter.StepSum;
    }

    public int startActionID = 0;
    public bool isDebugFloorLoaded => maps[MAX_FLOOR] != null;

    public int LastFloor => ResourceLoader.Instance.enemyTypesData.Length;

#if UNITY_EDITOR
    public bool isScenePlayedByEditor = true;
    public void SetDebugAction(int startActionID)
    {
        if (isScenePlayedByEditor) this.startActionID = startActionID;
    }
#endif

    public WorldMap Map(int floor, bool createNew = true)
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

            if (createNew) maps[floor - 1] = (maps[floor - 1] ?? new WorldMap(floor, size, size));

            return maps[floor - 1];
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
            2, 0, 2, 1, 1, 1, 4, 1,22, 1, 1, 1, 1,21, 2,
            2, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           23, 0, 4, 1, 1,21, 2, 1,22, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,22, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        var deadEnds = new Dictionary<Pos, IDirection>()
        {
            { new Pos( 5, 5),    Direction.west  },
            { new Pos( 3, 1),    Direction.north },
            { new Pos( 4, 1),    Direction.north },
            { new Pos( 5, 1),    Direction.north },
            { new Pos( 3, 2),    Direction.north },
            { new Pos( 4, 2),    Direction.north },
            { new Pos( 5, 2),    Direction.north },
            { new Pos( 3, 3),    Direction.north },
            { new Pos( 4, 3),    Direction.north },
            { new Pos( 9, 1),    Direction.north },
            { new Pos(10, 1),    Direction.north },
            { new Pos(11, 1),    Direction.north },
            { new Pos( 9, 2),    Direction.north },
            { new Pos(10, 2),    Direction.north },
            { new Pos(11, 2),    Direction.north },
            { new Pos( 9, 3),    Direction.north },
            { new Pos(10, 3),    Direction.north },
            { new Pos(11, 3),    Direction.north },
            { new Pos( 9, 4),    Direction.north },
            { new Pos(10, 4),    Direction.north },
            { new Pos(11, 4),    Direction.north },
            { new Pos( 9, 5),    Direction.north },
            { new Pos(10, 5),    Direction.north },
            { new Pos(11, 5),    Direction.north },
            { new Pos( 1, 3),    Direction.north },
            { new Pos( 1, 5),    Direction.north },
            { new Pos(13, 13),  Direction.north },
            { new Pos( 1, 1),    Direction.south },
        };

        var fixedMessagePos = new Dictionary<Pos, IDirection>() { { new Pos(0, 2), Direction.east } };

        maps[MAX_FLOOR] = new WorldMap(new MapManager(2, matrix, 15, deadEnds, fixedMessagePos));
    }

    private int[] FinalMapMatrix()
    {
        return new[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1,21, 2,21, 1, 1, 1,21, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 2, 1, 1, 1, 2, 0, 2,
            2, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 2, 2, 4, 2, 4, 2, 2, 2, 0, 2, 1,21, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 4, 1, 1,21, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 4, 1, 1, 1, 4, 0, 2,
            2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1,21, 1, 2, 1,21, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2,21, 1, 1, 1,21, 2,21, 1, 1, 1,21, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 4, 1, 1, 1, 4, 0, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 4, 2, 4, 2, 2, 2, 0, 2, 1,21, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 4, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 1, 2, 0, 2,
            2,21, 1, 1, 1, 1, 1,21, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 4, 1, 1, 1, 1,21, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,20, 2, 2, 2, 2, 2, 2, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 0, 4, 1, 1, 1, 1,21, 2,
            2, 4, 2, 2, 2, 2, 2, 2, 2, 0, 2, 1, 9, 1, 1, 1, 9, 1, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1,22, 9, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 0, 4, 1, 1, 1, 1,21, 2,
            2, 1, 1, 1, 1, 9, 3, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1,22, 9, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 0, 4, 1, 1, 1, 1,21, 2,
            2, 1, 1,22, 1, 9, 3, 1, 2, 0, 2, 1, 9, 1, 1, 1, 9, 1, 2, 0, 2, 0, 2, 2, 2, 2, 2, 2, 2,
            2, 1,22, 1,22,22, 9, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 0, 4, 1, 1, 1, 1,21, 2,
            2, 2, 2, 4, 2, 2, 2, 4, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 4, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 4, 0, 4, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,21, 1, 1, 1,21, 2, 2, 2,
            2, 0, 2, 0, 2, 0, 2, 0, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 2, 2, 2,
            2, 0, 2, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,21, 1, 1, 2, 2, 2,
            2, 4, 2, 2, 2, 4, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 2, 2, 2,
            2, 1, 1, 1, 1,21, 1, 1, 1,21, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2,21, 1, 1, 1,21, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1,21, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2,21, 1, 1, 1,21, 1, 1, 1,21, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };
    }

    private Dictionary<Pos, IDirection> FinalMapDeadEnds()
    {
        var randomPos = new Dictionary<Pos, IDirection>()
        {
            { new Pos(19,  1), Direction.west  },
            { new Pos(27,  1), Direction.south },
            { new Pos(27,  7), Direction.north },
            { new Pos(13,  1), Direction.east  },
        }
        .OrderBy(pos => Guid.NewGuid()).ToArray();

        return new Dictionary<Pos, IDirection>()
        {
            { new Pos(16, 12),  Direction.north     },
            { new Pos(16, 16),  Direction.north     },
            { new Pos(12, 16),  Direction.north     },
            { new Pos(12, 12),  Direction.north     },
            { new Pos( 3, 21),  Direction.north     },
            { new Pos(11, 27),  Direction.north     },
            { new Pos( 6, 13),  Direction.east      },
            { new Pos( 5, 14),  Direction.west      },
            { new Pos( 6, 15),  Direction.east      },
            { new Pos( 5, 16),  Direction.west      },
            { new Pos( 6, 17),  Direction.east      },
            { new Pos(13,  7),  Direction.west      },
            { randomPos[0].Key, randomPos[0].Value  }, // Battle Shield
            { randomPos[1].Key, randomPos[1].Value  }, // Jamadhar
            { randomPos[2].Key, randomPos[2].Value  }, // Coin
            { randomPos[3].Key, randomPos[3].Value  }, // TreasureKey
            { new Pos(12, 14),  Direction.north     }, // Coin
            { new Pos(14, 12),  Direction.north     }, // Coin
            { new Pos(16, 14),  Direction.north     }, // Coin
            { new Pos(14, 16),  Direction.north     }, // Coin
            { new Pos(14, 14),  Direction.north     }, // KeyBlade
            { new Pos( 1,  1),  Direction.south     }, // up stairs
        };
    }

    private Dictionary<Pos, IDirection> FinalMapFixedMessagePos()
    {
        return new Dictionary<Pos, IDirection>()
        {
            { new Pos(12, 10), Direction.north},
            { new Pos(22,  7), Direction.west },
        };
    }

    private Dictionary<Pos, IDirection> FinalMapBloodMessagePos()
    {
        return new Dictionary<Pos, IDirection>()
        {
            { new Pos(23, 8), Direction.south },
       };
    }

    public void ClearMaps()
    {
        maps = Enumerable.Repeat<WorldMap>(null, MAX_FLOOR + 1).ToArray();
        mapSize = Enumerable.Repeat(41, MAX_FLOOR + 1).ToArray();
        currentFloor = 1;

        mapSize[0] = 19;
        mapSize[1] = 31;
        mapSize[LastFloor - 1] = 29;

        maps[LastFloor - 1] = new WorldMap(new MapManager(LastFloor, FinalMapMatrix(), 29, FinalMapDeadEnds(), FinalMapFixedMessagePos(), FinalMapBloodMessagePos()));
    }

    public void InitData(bool clearMaps = true)
    {
        if (clearMaps) ClearMaps();

        moneyAmount = 0;
        mapComp = 0f;
        endTimeSec = 0;
        defeatCount = 0;
        level = 1;
        strength = 10;
        magic = 10;
        clearRank = 0;
        clearRecord = null;

        treasureComp = 0f;
        title = "なし";
        titleBonusRatio = 1f;
        steps = 0;
        storedTimeSec = 0;
    }

    public DataStoreAgent.MapData[] ExportMapData()
    {
        var export = Enumerable.Repeat<DataStoreAgent.MapData>(null, LastFloor).ToArray();
        for (int i = 0; i < LastFloor; i++)
        {
            if (maps[i] == null) continue;
            if (i + 1 == currentFloor) maps[i].StoreTileStateData();

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

            if (mapData != null)
            {
                maps[i] = new WorldMap(new MapManager(i + 1, mapData));
                maps[i].ImportMapData(mapData);
            }
            else if (i + 1 == currentFloor)
            {
                var map = Map(currentFloor);
                import.playerData.kvPosDir = map.StairsBottom;
            }
        }

        WorldMap.isExitDoorLocked = import.isExitDoorLocked;
        WorldMap.exitDoorPos = import.exitDoorPos;
    }
}
