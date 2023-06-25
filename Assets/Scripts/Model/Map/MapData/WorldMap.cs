using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap : TileMapData
{
    public int floor { get; protected set; } = 0;

    public MiniMapData miniMapData { get; protected set; }
    public void ClearCurrentViewOpen() => miniMapData.ClearCurrentViewOpen();

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
    public List<Pos> roomCenterPos { get; private set; }
    public List<Pos> fixedMessagePos { get; private set; }
    public List<Pos> bloodMessagePos { get; private set; }
    public Dictionary<Pos, int> randomMessagePos { get; private set; } = new Dictionary<Pos, int>();

    public static bool isExitDoorLocked = true;
    private List<Pos> tileOpenPosList = null;
    private List<Pos> tileBrokenPosList = null;

    public StairsMapData stairsMapData { get; private set; }
    public DirMapHandler dirMapHandler { get; private set; }
    private RawMapData rawMapData;

    public Terrain[,] CloneMatrix() => dirMapHandler.matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => dirMapHandler.dirMap.Clone() as Dir[,];
    public int[] ConvertMapData() => dirMapHandler.ConvertMapData();
    public int[] ConvertDirData() => dirMapHandler.ConvertDirData();

    public Dir GetDoorDir(int x, int y) => rawMapData.GetDoorDir(x, y);
    public Dir GetPillarDir(int x, int y) => dirMapHandler.GetPillarDir(x, y);

    public ITile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public ITile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public ITile GetTile(int x, int y) => IsOutOfRange(x, y) ? new Wall() : matrix[x, y];
    public bool Unlock(Pos pos) => dirMapHandler.Unlock(pos);

    /// <summary>
    /// The tile can be seen through
    /// </summary>
    public bool IsTileViewOpen(int x, int y) => !IsOutOfRange(x, y) && matrix[x, y].IsViewOpen;

    public Pos GetGroundPos(Pos targetPos, List<Pos> placeAlready)
    {
        List<Pos> exceptFor = new List<Pos>(placeAlready);
        exceptFor.AddRange(new Pos[] { stairsTop.Key, stairsBottom.Key });

        for (int range = 0; range < 3; range++)
        {
            var spacePos = SearchSpaceNearBy(targetPos, range, exceptFor);
            if (!spacePos.IsNull) return spacePos;
        }

        return new Pos();
    }

    public void ForEachTiles(Action<ITile> action)
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                action(matrix[i, j]);
            }
        }
    }
    public void ForEachTiles(Action<ITile, Pos> action)
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                action(matrix[i, j], new Pos(i, j));
            }
        }
    }

    public void ClearCharacterOnTileInfo()
    {
        ForEachTiles(tile => tile.OnCharacterDest = tile.OnEnemy = tile.AboveEnemy = null);
    }

    public void ApplyTileState()
    {
        if (tileOpenPosList != null) tileOpenPosList.ForEach(pos => (matrix[pos.x, pos.y] as IOpenable).Open());
        if (tileBrokenPosList != null) tileBrokenPosList.ForEach(pos => (matrix[pos.x, pos.y] as Door).Break());

        if (floor == 1 && !isExitDoorLocked)
        {
            Pos pos = stairsMapData.exitDoor;
            var exitDoor = (matrix[pos.x, pos.y] as ExitDoor);
            if (!exitDoor.IsOpen) exitDoor.Unlock();
        }
    }

    public void StoreTileStateData()
    {
        (tileOpenPosList, tileBrokenPosList) = RetrieveTileStateData();

        if (floor == 1)
        {
            Pos pos = stairsMapData.exitDoor;
            isExitDoorLocked = (matrix[pos.x, pos.y] as ExitDoor).IsLocked;
        }
    }

    public (List<Pos>, List<Pos>) ExportTileStateData()
    {
        return (tileOpenPosList == null || tileBrokenPosList == null)
            ? RetrieveTileStateData() : (tileOpenPosList, tileBrokenPosList);
    }

    private (List<Pos>, List<Pos>) RetrieveTileStateData()
    {
        var open = new List<Pos>();
        var broken = new List<Pos>();

        // FIXME: use random message count as initialized flag for now.
        bool isTilesReady = randomMessagePos.Count() > 0;
        if (isTilesReady)
        {
            ForEachTiles((tile, pos) =>
            {
                if (tile is IOpenable)
                {
                    if (tile is Door && (tile as Door).IsBroken)
                    {
                        broken.Add(pos);
                    }
                    else if ((tile as IOpenable).IsOpen)
                    {
                        open.Add(pos);
                    }
                }
            });
        }

        return (open, broken);
    }


    public Pos SearchSpaceNearBy(Pos targetPos, int range = 2, List<Pos> exceptFor = null)
    {
        var spaceCandidates = new List<Pos>();

        for (int j = targetPos.y - range; j < targetPos.y + range; j++)
        {
            for (int i = targetPos.x - range; i < targetPos.x + range; i++)
            {
                var pos = new Pos(i, j);
                if (IsEnterableTile(pos)) spaceCandidates.Add(pos);
            }
        }

        exceptFor?.ForEach(pos => spaceCandidates.Remove(pos));

        return spaceCandidates.Count > 0 ? spaceCandidates.GetRandom() : new Pos();
    }

    /// <summary>
    /// Tiles that cannot enter onto or move from are not enterable for enemies.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>TRUE if enterable</returns>
    private bool IsEnterableTile(Pos pos)
    {
        if (!GetTile(pos).IsEnterable()) return false;

        if (GetTile(pos.DecY()).IsEnterable()) return true; // North is open
        if (GetTile(pos.IncX()).IsEnterable()) return true; // East is open
        if (GetTile(pos.IncY()).IsEnterable()) return true; // South is open
        if (GetTile(pos.DecX()).IsEnterable()) return true; // West is open

        return false;
    }

    // Create new map data
    public static WorldMap Create(int floor = 1, int width = 49, int height = 49)
    {
        var maze = new MazeCreator(width, height);
        var dirMapData = new DirMapData(maze.matrix, width, height);
        var dirMapHandler = new DirMapHandler(dirMapData);
        var stairsMapData = new StairsMapData(dirMapData, floor);
        var pitMessageMapData = new PitMessageMapData(stairsMapData, floor);

        return new WorldMap(floor, maze.roomCenterPos, dirMapHandler, stairsMapData, pitMessageMapData);
    }

    // Create from custom map data
    public static WorldMap Create(CustomMapData data)
    {
        var dirMapData = new DirMapData(data);
        var dirMapHandler = new DirMapHandler(dirMapData);
        var stairsMapData = new StairsMapData(dirMapData, data);
        var pitMessageMapData = new PitMessageMapData(dirMapHandler, data);

        return new WorldMap(data.floor, data.roomCenter, dirMapHandler, stairsMapData, pitMessageMapData);
    }

    // Import from MapData
    public static WorldMap Import(int floor, DataStoreAgent.MapData import)
    {
        var dirMapData = new DirMapData(import);
        var dirMapHandler = new DirMapHandler(dirMapData);

        return new WorldMap(floor, dirMapHandler, import);
    }

    // Create map data
    protected WorldMap(int floor, List<Pos> roomCenterPos, DirMapHandler dirMapHandler, StairsMapData stairsMapData, PitMessageMapData pitMessageMapData)
        : base(null, dirMapHandler.width, dirMapHandler.height)
    {
        this.floor = floor;

        this.dirMapHandler = dirMapHandler;
        this.rawMapData = dirMapHandler.rawMapData;
        this.stairsMapData = stairsMapData;

        this.stairsBottom = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsBottom, stairsMapData.UpStairsDir);
        this.stairsTop = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsTop, stairsMapData.DownStairsDir);

        this.deadEndPos = new Dictionary<Pos, IDirection>(stairsMapData.deadEndPos);
        this.roomCenterPos = new List<Pos>(roomCenterPos);
        this.fixedMessagePos = new List<Pos>(pitMessageMapData.fixedMessagePos);
        this.bloodMessagePos = new List<Pos>(pitMessageMapData.bloodMessagePos);

        // Generate tile matrix by MiniMapData constructor to save a for-loop to convert terrain to mini map color.
        miniMapData = MiniMapData.Convert(dirMapHandler.matrix, width, height);
        matrix = miniMapData.matrix;
    }


    // Import map data
    protected WorldMap(int floor, DirMapHandler dirMapHandler, DataStoreAgent.MapData import)
        : base(null, dirMapHandler.width, dirMapHandler.height)
    {
        this.floor = floor;

        this.dirMapHandler = dirMapHandler;
        this.rawMapData = dirMapHandler.rawMapData;

        this.stairsMapData = new StairsMapData(dirMapHandler, import);

        this.stairsBottom = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsBottom, stairsMapData.UpStairsDir);
        this.stairsTop = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsTop, stairsMapData.DownStairsDir);

        this.roomCenterPos = import.roomCenterPos.ToList();
        this.tileOpenPosList = import.tileOpenData.ToList();
        this.tileBrokenPosList = import.tileBrokenData.ToList();

        this.fixedMessagePos = import.fixedMessagePos.ToList();
        this.bloodMessagePos = import.bloodMessagePos.ToList();
        for (int i = 0; i < import.randomMessagePos.Length; i++)
        {
            var posList = import.randomMessagePos[i];
            posList.pos.ForEach(pos => this.randomMessagePos[pos] = i);
        }

        // Generate tile matrix by MiniMapData constructor to save a for-loop to convert terrain to mini map color.
        miniMapData = MiniMapData.Convert(dirMapHandler.matrix, width, height);
        matrix = miniMapData.matrix;

        miniMapData.ImportTileDiscoveredData(import.tileDiscoveredData);
    }

    public Dir SetTerrain(int x, int y, Terrain terrain)
    {
        Dir dir = dirMapHandler.SetTerrain(x, y, terrain);

        ITile temp = matrix[x, y];
        matrix[x, y] = TileMapData.ConvertToTile(terrain);
        matrix[x, y].items = temp.items;

        miniMapData.SetTerrain(x, y, terrain);
        return dir;
    }

    public bool IsCurrentViewOpen(Vector3 worldPos) => miniMapData.IsCurrentViewOpen(worldPos);
    public Texture2D GetMiniMap(int mapSize = 15) => miniMapData.GetMiniMap(mapSize);
    public Vector3 MiniMapCenterWorldPos(int mapSize = 15) => miniMapData.MiniMapCenterWorldPos(mapSize);
    public void SetDiscovered(Pos pos) => miniMapData.SetDiscovered(pos);
    public int SumUpDiscovered() => miniMapData.SumUpDiscovered();
    public bool[] ExportTileDiscoveredData() => miniMapData.ExportTileDiscoveredData();

    public KeyValuePair<Pos, IDirection> stairsBottom { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);
    public KeyValuePair<Pos, IDirection> stairsTop { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);

    public KeyValuePair<Pos, IDirection> StairsEnter(bool isDownStairs) => isDownStairs ? stairsTop : stairsBottom;
    public KeyValuePair<Pos, IDirection> StairsExit(bool isDownStairs) => StairsEnter(!isDownStairs);

    public DataStoreAgent.PosList[] ExportRandomMessagePos()
    {
        // export[RANDOM_MESSAGE_ID] = List<PLACED_BOARD_POSITION>
        var export = Enumerable.Repeat(new List<Pos>(), ResourceLoader.Instance.floorMessagesData.Param(floor - 1).randomMessages.Length).ToArray();
        randomMessagePos.ForEach(kv => export[kv.Value].Add(kv.Key));
        return export.Select(posList => new DataStoreAgent.PosList(posList)).ToArray();
    }

}
