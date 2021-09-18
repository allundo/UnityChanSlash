using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap
{
    public static readonly float TILE_UNIT = 2.5f;

    public Tile[,] tileInfo { get; protected set; }
    public Texture2D texMap { get; protected set; }
    public bool[,] discovered { get; protected set; }

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }

    private MapManager map;
    public Terrain[,] CloneMatrix() => map.matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => map.dirMap.Clone() as Dir[,];
    public Dictionary<Pos, IDirection> CopyDeadEndPos() => new Dictionary<Pos, IDirection>(map.deadEndPos);
    public Dir GetPallDir(int x, int y) => map.GetPallDir(x, y);

    public Tile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public Tile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public Tile GetTile(int x, int y) => IsOutWall(x, y) ? new Wall() : tileInfo[x, y];

    public Ground GetGround(ref int x, ref int y)
    {
        Tile tile = GetTile(x, y);

        if (tile is Ground) return tile as Ground;

        for (int j = y - 1; j < y + 1; j++)
        {
            for (int i = x - 1; i < x + 1; i++)
            {
                tile = GetTile(i, j);

                if (tile is Ground)
                {
                    x = i;
                    y = j;
                    return tile as Ground;
                }

            }
        }
        return null;
    }

    public bool IsOutWall(int x, int y) => x <= 0 || y <= 0 || x >= Width - 1 || y >= Height - 1;

    public int Width { get; protected set; } = 49;
    public int Height { get; protected set; } = 49;

    public WorldMap()
    {
        map = new MapManager().SetStair();

        Width = map.width;
        Height = map.height;

        tileInfo = new Tile[Width, Height];

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
                    case Terrain.Pall:
                        tileInfo[i, j] = new Wall();
                        pixels[i + Width * j] = Color.gray;
                        break;

                    case Terrain.Door:
                        tileInfo[i, j] = new Door();
                        pixels[i + Width * j] = Color.red;
                        break;

                    case Terrain.Stair:
                        tileInfo[i, j] = new Stair();
                        pixels[i + Width * j] = Color.blue;
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
    public bool IsTileViewOpen(int x, int y) => IsOutWall(x, y) ? false : tileInfo[x, y].IsViewOpen;

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
        Pos pos = GameManager.Instance.PlayerPos;
        int half = mapSize / 2;

        return new Pos(
            Mathf.Clamp(pos.x, half, Width - half - 1),
            Mathf.Clamp(pos.y, half, Height - half - 1)
        );
    }
    public Vector3 MiniMapCenterWorldPos(int mapSize = 15) => WorldPos(MiniMapCenterPos(mapSize));

    public void SetDiscovered(Pos pos) => SetDiscovered(pos.x, pos.y);
    public void SetDiscovered(int x, int y)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                discovered[i, j] = true;
            }
        }
    }

    public bool[,] GetDiscovered(int x, int y, int blockWidth, int blockHeight)
    {
        var discovered = new bool[blockWidth, blockHeight];

        for (int j = 0; j < blockHeight; j++)
        {
            for (int i = 0; i < blockWidth; i++)
            {
                discovered[i, j] = this.discovered[x + i, y + j];
            }
        }

        return discovered;
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
                Tile tile = tileInfo[x + i, y + j];

                pixels[i + inverseJ * blockWidth] =
                    tile is Door && (tile as Door).IsOpen ?
                        new Color(0.75f, 0, 0, 1f) : texPixels[i + j * blockWidth];
            }
        }

        return pixels;
    }

    // FIXME
    public Pos InitPos
    {
        get
        {
            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (tileInfo[i, j] is Ground) return new Pos(i, j);
                }
            }
            return new Pos();
        }
    }

    // FIXME
    public IDirection InitDir => new South();
}