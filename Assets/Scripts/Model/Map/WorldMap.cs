using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap
{
    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    public int floor { get; protected set; } = 0;
    public ITile[,] tileInfo { get; protected set; }
    public Texture2D texMap { get; protected set; }
    public bool[,] discovered { get; protected set; }
    private List<Pos> currentViewOpen = new List<Pos>();
    public void ClearCurrentViewOpen() => currentViewOpen.Clear();

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
    public List<Pos> roomCenterPos { get; private set; }
    public List<Pos> fixedMessagePos { get; private set; }
    public List<Pos> bloodMessagePos { get; private set; }
    public Dictionary<Pos, int> randomMessagePos { get; private set; } = new Dictionary<Pos, int>();

    public static bool isExitDoorLocked = true;
    private List<Pos> tileOpenPosList = null;
    private List<Pos> tileBrokenPosList = null;

    private DirMapHandler dirMapHandler;
    private RawMapData rawMapData;

    public Terrain[,] CloneMatrix() => dirMapHandler.matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => dirMapHandler.dirMap.Clone() as Dir[,];
    public int[] ConvertMapData() => dirMapHandler.ConvertMapData();
    public int[] ConvertDirData() => dirMapHandler.ConvertDirData();

    public Dir GetDoorDir(int x, int y) => rawMapData.GetDoorDir(x, y);
    public Dir GetPillarDir(int x, int y) => dirMapHandler.GetPillarDir(x, y);

    public ITile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public ITile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public ITile GetTile(int x, int y) => IsOutOfRange(x, y) ? new Wall() : tileInfo[x, y];
    public bool Unlock(Pos pos) => dirMapHandler.Unlock(pos);

    public StairsData stairsData { get; private set; }

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
                action(tileInfo[i, j]);
            }
        }
    }
    public void ForEachTiles(Action<ITile, Pos> action)
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                action(tileInfo[i, j], new Pos(i, j));
            }
        }
    }

    public void ClearCharacterOnTileInfo()
    {
        ForEachTiles(tile => tile.OnCharacterDest = tile.OnEnemy = tile.AboveEnemy = null);
    }

    public void ApplyTileState()
    {
        if (tileOpenPosList != null) tileOpenPosList.ForEach(pos => (tileInfo[pos.x, pos.y] as IOpenable).Open());
        if (tileBrokenPosList != null) tileBrokenPosList.ForEach(pos => (tileInfo[pos.x, pos.y] as Door).Break());

        if (floor == 1 && !isExitDoorLocked)
        {
            Pos pos = stairsData.exitDoor;
            var exitDoor = (tileInfo[pos.x, pos.y] as ExitDoor);
            if (!exitDoor.IsOpen) exitDoor.Unlock();
        }
    }

    public void StoreTileStateData()
    {
        (tileOpenPosList, tileBrokenPosList) = RetrieveTileStateData();

        if (floor == 1)
        {
            Pos pos = stairsData.exitDoor;
            isExitDoorLocked = (tileInfo[pos.x, pos.y] as ExitDoor).IsLocked;
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



    public bool IsOutOfRange(int x, int y) => x < 0 || y < 0 || x >= width || y >= height;

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

    public int width { get; protected set; } = 49;
    public int height { get; protected set; } = 49;

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
    public static WorldMap Import(int floor, DataStoreAgent.MapData mapData)
    {
        var dirMapData = new DirMapData(mapData);
        var dirMapHandler = new DirMapHandler(dirMapData);
        var stairsMapData = new StairsMapData(dirMapData, mapData);
        var pitMessageMapData = new PitMessageMapData(dirMapData);

        var map = new WorldMap(floor, mapData.roomCenterPos.ToList(), dirMapHandler, stairsMapData, pitMessageMapData);
        map.ImportMapData(mapData);
        return map;
    }

    private void ImportMapData(DataStoreAgent.MapData import)
    {
        tileOpenPosList = import.tileOpenData.ToList();
        tileBrokenPosList = import.tileBrokenData.ToList();

        fixedMessagePos = import.fixedMessagePos.ToList();
        bloodMessagePos = import.bloodMessagePos.ToList();
        for (int i = 0; i < import.randomMessagePos.Length; i++)
        {
            var posList = import.randomMessagePos[i];
            posList.pos.ForEach(pos => randomMessagePos[pos] = i);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                discovered[x, y] = import.tileDiscoveredData[x + width * y];
            }
        }
    }

    protected WorldMap(int floor, List<Pos> roomCenterPos, DirMapHandler dirMapHandler, StairsMapData stairsMapData, PitMessageMapData pitMessageMapData)
    {
        this.floor = floor;
        this.width = dirMapHandler.width;
        this.height = dirMapHandler.height;

        this.dirMapHandler = dirMapHandler;
        this.rawMapData = dirMapHandler.rawMapData;
        this.stairsData = new StairsData(stairsMapData);

        this.stairsBottom = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsBottom, stairsMapData.UpStairsDir);
        this.stairsTop = new KeyValuePair<Pos, IDirection>(stairsMapData.StairsTop, stairsMapData.DownStairsDir);

        this.deadEndPos = new Dictionary<Pos, IDirection>(stairsMapData.deadEndPos);
        this.roomCenterPos = new List<Pos>(roomCenterPos);
        this.fixedMessagePos = new List<Pos>(pitMessageMapData.fixedMessagePos);
        this.bloodMessagePos = new List<Pos>(pitMessageMapData.bloodMessagePos);

        tileInfo = new ITile[width, height];

        texMap = new Texture2D(width, height, TextureFormat.RGB24, false);

        var pixels = texMap.GetPixels();
        var matrix = CloneMatrix();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                (tileInfo[i, j], pixels[i + width * j]) = ConvertTerrain(matrix[i, j]);
            }
        }

        // The elements are initialized by false automatically
        discovered = new bool[width, height];

        texMap.SetPixels(pixels);
        texMap.Apply();
    }

    private (ITile, Color) ConvertTerrain(Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.Wall:
            case Terrain.Pillar:
                return (new Wall(), Color.gray);

            case Terrain.MessageWall:
            case Terrain.MessagePillar:
            case Terrain.BloodMessageWall:
            case Terrain.BloodMessagePillar:
                return (new MessageWall(), Color.gray);

            case Terrain.Door:
            case Terrain.LockedDoor:
                return (new Door(), Color.red);

            case Terrain.ExitDoor:
                return (new ExitDoor(), Color.green);

            case Terrain.DownStairs:
            case Terrain.UpStairs:
                return (new Stairs(), Color.green);

            case Terrain.Box:
                return (new Box(), Color.blue);

            case Terrain.Pit:
                return (new Pit(), Color.black);

            default:
                return (new Ground(), Color.black);
        }
    }

    public Dir SetTerrain(int x, int y, Terrain terrain)
    {
        Dir dir = dirMapHandler.SetTerrain(x, y, terrain);

        var pixels = texMap.GetPixels();

        ITile temp = tileInfo[x, y];
        (tileInfo[x, y], pixels[x + width * y]) = ConvertTerrain(terrain);
        tileInfo[x, y].items = temp.items;

        texMap.SetPixels(pixels);
        texMap.Apply();

        return dir;
    }

    public Vector3 WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public Vector3 WorldPos(int x, int y) => new Vector3((0.5f + x - width * 0.5f) * TILE_UNIT, 0.0f, (-0.5f - y + height * 0.5f) * TILE_UNIT);

    public Pos MapPos(Vector3 pos) =>
        new Pos(
            (int)Math.Round((width - 1) * 0.5f + pos.x / TILE_UNIT, MidpointRounding.AwayFromZero),
            (int)Math.Round((height - 1) * 0.5f - pos.z / TILE_UNIT, MidpointRounding.AwayFromZero)
        );

    /// <summary>
    /// The tile can be seen through
    /// </summary>
    public bool IsTileViewOpen(int x, int y) => IsOutOfRange(x, y) ? false : tileInfo[x, y].IsViewOpen;

    public bool IsCurrentViewOpen(Vector3 worldPos) => IsCurrentViewOpen(MapPos(worldPos));
    public bool IsCurrentViewOpen(Pos pos) => currentViewOpen.Contains(pos);

    public Texture2D GetMiniMap(int mapSize = 15)
    {
        Pos pos = MiniMapCenterPos(mapSize);

        int x = pos.x - mapSize / 2;
        int y = pos.y - mapSize / 2;

        Color[] pixels = texMap.GetPixels(x, y, mapSize, mapSize);

        var texture = new Texture2D(mapSize, mapSize);
        texture.SetPixels(MiniMapPixels(pixels, x, y, mapSize, mapSize));
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;
    }

    private Pos MiniMapCenterPos(int mapSize = 15)
    {
        Pos pos = PlayerInfo.Instance.Pos;
        int half = mapSize / 2;

        return new Pos(
            Mathf.Clamp(pos.x, half, width - half - 1),
            Mathf.Clamp(pos.y, half, height - half - 1)
        );
    }

    /// <summary>
    /// Convert mini map center TILE POSITION to world position by Vector3. <br />
    /// The mini map size must be 2n + 1 to adjust center position to a tile center.
    /// </summary>
    /// <param name="mapSize">!Caution! : accepts only 2n + 1 size</param>
    /// <returns></returns>
    public Vector3 MiniMapCenterWorldPos(int mapSize = 15) => WorldPos(MiniMapCenterPos(mapSize));

    public void SetDiscovered(Pos pos)
    {
        currentViewOpen.Add(pos);

        for (int i = pos.x - 1; i <= pos.x + 1; i++)
        {
            for (int j = pos.y - 1; j <= pos.y + 1; j++)
            {
                discovered[i, j] = true;
            }
        }
    }

    public int SumUpDiscovered()
    {
        int discoveredCount = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (discovered[x, y]) discoveredCount++;
            }
        }
        return discoveredCount;
    }

    private Color[] MiniMapPixels(Color[] texPixels, int x, int y, int blockWidth, int blockHeight)
    {
        Color[] pixels = Enumerable.Repeat(Color.clear, texPixels.Length).ToArray();

        for (int j = 0; j < blockHeight; j++)
        {
            for (int i = 0; i < blockWidth; i++)
            {
                if (!discovered[x + i, y + j]) continue;

                int inverseJ = blockHeight - j - 1;
                ITile tile = tileInfo[x + i, y + j];

                pixels[i + inverseJ * blockWidth] =
                    tile is Door && (tile as Door).IsOpen ?
                        new Color(0.75f, 0, 0, 1f) : texPixels[i + j * blockWidth];
            }
        }

        return pixels;
    }

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

    public bool[] ExportTileDiscoveredData()
    {
        var export = new bool[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                export[x + width * y] = discovered[x, y];
            }
        }
        return export;
    }
}
