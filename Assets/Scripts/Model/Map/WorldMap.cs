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
    public List<Pos> currentViewOpen { get; protected set; } = new List<Pos>();

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
    public List<Pos> roomCenterPos { get; private set; }
    public List<Pos> fixedMessagePos { get; private set; }
    public Dictionary<Pos, int> randomMessagePos { get; private set; } = new Dictionary<Pos, int>();

    private List<Pos> openReservedTilePos = null;

    private MapManager map;
    public Terrain[,] CloneMatrix() => map.matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => map.dirMap.Clone() as Dir[,];
    public int[] GetMapMatrix() => map.GetMapData();
    public int[] GetDirData() => map.GetDirData();

    public Dir GetDoorDir(int x, int y) => map.GetDoorDir(x, y);
    public Dir GetPillarDir(int x, int y) => map.GetPillarDir(x, y);
    public Dir GetValidDir(int x, int y) => map.GetValidDir(x, y);

    public IDirection exitDoorDir => map.exitDoorDir;

    public ITile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public ITile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public ITile GetTile(int x, int y) => IsOutOfRange(x, y) ? new Wall() : tileInfo[x, y];

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
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                action(tileInfo[i, j]);
            }
        }
    }
    public void ForEachTiles(Action<ITile, Pos> action)
    {
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                action(tileInfo[i, j], new Pos(i, j));
            }
        }
    }

    public void ApplyTileOpen()
    {
        if (openReservedTilePos != null)
        {
            openReservedTilePos.ForEach(pos => (tileInfo[pos.x, pos.y] as IOpenable).Open());
            openReservedTilePos = null;
            return;
        }

        ForEachTiles(tile =>
        {
            if (tile is IOpenable)
            {
                var openTile = tile as IOpenable;
                if (openTile.IsOpen) openTile.Open();
            }
        });
    }

    public List<Pos> ExportTileOpenData()
    {
        if (openReservedTilePos != null) return openReservedTilePos;

        var data = new List<Pos>();

        if (randomMessagePos.Count() > 0/* FIXME: use random message count as initialized flag for now */)
        {
            ForEachTiles((tile, pos) =>
            {
                if (tile is IOpenable && (tile as IOpenable).IsOpen) data.Add(pos);
            });
        }

        return data;
    }

    public void ImportMapData(DataStoreAgent.MapData import)
    {
        openReservedTilePos = import.tileOpenData.ToList();
        fixedMessagePos = import.fixedMessagePos.ToList();
        for (int i = 0; i < import.randomMessagePos.Length; i++)
        {
            var posList = import.randomMessagePos[i];
            posList.pos.ForEach(pos => randomMessagePos[pos] = i);
        }
    }

    public bool IsOutOfRange(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height;

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

    public int Width { get; protected set; } = 49;
    public int Height { get; protected set; } = 49;

    public WorldMap(MapManager map)
    {
        this.map = map;

        floor = map.floor;
        stairsBottom = map.stairsBottom;

        stairsTop = map.stairsTop;
        deadEndPos = new Dictionary<Pos, IDirection>(this.map.deadEndPos);
        roomCenterPos = new List<Pos>(this.map.roomCenterPos);
        fixedMessagePos = new List<Pos>(this.map.fixedMessagePos);

        Width = map.width;
        Height = map.height;

        tileInfo = new ITile[Width, Height];

        texMap = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        var pixels = texMap.GetPixels();

        discovered = new bool[Width, Height];

        var matrix = CloneMatrix();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                discovered[i, j] = false;

                switch (matrix[i, j])
                {
                    case Terrain.Wall:
                    case Terrain.Pillar:
                        tileInfo[i, j] = new Wall();
                        pixels[i + Width * j] = Color.gray;
                        break;

                    case Terrain.MessageWall:
                    case Terrain.MessagePillar:
                        tileInfo[i, j] = new MessageWall();
                        pixels[i + Width * j] = Color.magenta;
                        break;

                    case Terrain.Door:
                        tileInfo[i, j] = new Door();
                        pixels[i + Width * j] = Color.red;
                        break;

                    case Terrain.ExitDoor:
                        tileInfo[i, j] = new ExitDoor();
                        pixels[i + Width * j] = Color.green;
                        break;

                    case Terrain.DownStairs:
                    case Terrain.UpStairs:
                        tileInfo[i, j] = new Stairs();
                        pixels[i + Width * j] = Color.green;
                        break;

                    case Terrain.Box:
                        tileInfo[i, j] = new Box();
                        pixels[i + Width * j] = Color.blue;
                        break;

                    case Terrain.Pit:
                        tileInfo[i, j] = new Pit();
                        pixels[i + Width * j] = Color.black;
                        break;

                    default:
                        tileInfo[i, j] = new Ground();
                        pixels[i + Width * j] = Color.black;
                        break;
                }
            }
        }

        texMap.SetPixels(pixels);
        texMap.Apply();
    }

    public WorldMap(int floor = 1, int w = 49, int h = 49) : this(new MapManager(floor, w, h))
    { }

    public Vector3 WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public Vector3 WorldPos(int x, int y) => new Vector3((0.5f + x - Width * 0.5f) * TILE_UNIT, 0.0f, (-0.5f - y + Height * 0.5f) * TILE_UNIT);

    public Pos MapPos(Vector3 pos) =>
        new Pos(
            (int)Math.Round((Width - 1) * 0.5f + pos.x / TILE_UNIT, MidpointRounding.AwayFromZero),
            (int)Math.Round((Height - 1) * 0.5f - pos.z / TILE_UNIT, MidpointRounding.AwayFromZero)
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
        Pos pos = PlayerInfo.Instance.PlayerPos;
        int half = mapSize / 2;

        return new Pos(
            Mathf.Clamp(pos.x, half, Width - half - 1),
            Mathf.Clamp(pos.y, half, Height - half - 1)
        );
    }
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
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
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

    public KeyValuePair<Pos, IDirection> StairsBottom
    {
        get
        {
            if (!stairsBottom.Key.IsNull) return stairsBottom;

            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (tileInfo[i, j] is Ground)
                    {
                        stairsBottom = new KeyValuePair<Pos, IDirection>(new Pos(i, j), null);
                        return stairsBottom;
                    }
                }
            }

            return stairsBottom;
        }
        set { stairsBottom = value; }
    }
    private KeyValuePair<Pos, IDirection> stairsBottom = new KeyValuePair<Pos, IDirection>(new Pos(), null);
    public KeyValuePair<Pos, IDirection> stairsTop { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);

    public DataStoreAgent.PosList[] ExportRandomMessagePos()
    {
        var export = Enumerable.Repeat(new List<Pos>(), ResourceLoader.Instance.floorMessagesData.Param(floor - 1).randomMessages.Length).ToArray();
        randomMessagePos.ForEach(kv => export[kv.Value].Add(kv.Key));
        return export.Select(posList => new DataStoreAgent.PosList(posList)).ToArray();
    }
}
