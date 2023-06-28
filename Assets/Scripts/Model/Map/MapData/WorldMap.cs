using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap : TileMapHandler
{
    public MiniMapData miniMapData { get; protected set; }
    public TileStateHandler tileStateHandler { get; private set; }

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
    public List<Pos> roomCenterPos { get; private set; }

    public PitMessageMapData messagePosData { get; private set; }
    public StairsMapData stairsMapData { get; private set; }
    public DirMapHandler dirMapHandler { get; private set; }

    public Terrain[,] CloneMatrix() => dirMapHandler.matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => dirMapHandler.dirMap.Clone() as Dir[,];

    public ITile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public ITile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public ITile GetTile(int x, int y) => IsOutOfRange(x, y) ? new Wall() : matrix[x, y];

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

    public void ClearCharacterOnTileInfo()
    {
        ForEachTiles(tile => tile.OnCharacterDest = tile.OnEnemy = tile.AboveEnemy = null);
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
        var stairsMapData = new StairsMapData(dirMapData, import);
        var pitMessageMapData = new PitMessageMapData(dirMapData, floor, import);
        var map = new WorldMap(floor, import.roomCenterPos.ToList(), dirMapHandler, stairsMapData, pitMessageMapData);
        map.ImportTileData(import.tileOpenData, import.tileBrokenData, import.messageReadData, import.tileDiscoveredData);

        return map;
    }

    private void ImportTileData(Pos[] open, Pos[] broken, Pos[] read, bool[] discovered)
    {
        tileStateHandler.Import(open, broken, read);
        miniMapData.ImportTileDiscoveredData(discovered);
    }

    // Create map data
    protected WorldMap(int floor, List<Pos> roomCenterPos, DirMapHandler dirMapHandler, StairsMapData stairsMapData, PitMessageMapData pitMessageMapData)
        : base(null, floor, dirMapHandler.width, dirMapHandler.height)
    {
        this.dirMapHandler = dirMapHandler;
        this.stairsMapData = stairsMapData;
        this.messagePosData = pitMessageMapData;

        this.stairsBottom = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsBottom, stairsMapData.UpStairsDir);
        this.stairsTop = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsTop, stairsMapData.DownStairsDir);

        this.deadEndPos = new Dictionary<Pos, IDirection>(stairsMapData.deadEndPos);
        this.roomCenterPos = new List<Pos>(roomCenterPos);

        // Generate tile matrix by MiniMapData constructor to save a for-loop to convert terrain to mini map color.
        miniMapData = MiniMapData.Convert(dirMapHandler.matrix, floor, width, height);
        matrix = miniMapData.matrix;

        pitMessageMapData.ApplyMessages(matrix);

        tileStateHandler = new TileStateHandler(matrix, floor, width, height);
        if (floor == 1) tileStateHandler.SetExitDoor(stairsMapData.exitDoor);
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

    public KeyValuePair<Pos, IDirection> stairsBottom { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);
    public KeyValuePair<Pos, IDirection> stairsTop { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);

    public KeyValuePair<Pos, IDirection> StairsEnter(bool isDownStairs) => isDownStairs ? stairsTop : stairsBottom;
    public KeyValuePair<Pos, IDirection> StairsExit(bool isDownStairs) => StairsEnter(!isDownStairs);
}
