using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldMap
{
    public static readonly float TILE_UNIT = 2.5f;

    public Tile[,] tileInfo { get; protected set; }
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

        var matrix = CloneMatrix();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                switch (matrix[i, j])
                {
                    case Terrain.Wall:
                    case Terrain.Pall:
                        tileInfo[i, j] = new Wall();
                        break;

                    case Terrain.Door:
                        tileInfo[i, j] = new Door();
                        break;

                    case Terrain.Stair:
                        tileInfo[i, j] = new Stair();
                        break;

                    default:
                        tileInfo[i, j] = new Ground();
                        break;
                }
            }
        }
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
    public bool IsPlayerRange(int x, int y)
    {
        Pos pos = GameManager.Instance.PlayerPos;
        return x >= pos.x - 5 && x < pos.x + 5 && y >= pos.y - 5 && y < pos.y + 5;
    }

    public Vector3 GetRespawnPos()
    {
        while (true)
        {
            int rndX = Random.Range(0, Width);
            int rndY = Random.Range(0, Height);

            if (IsPlayerRange(rndX, rndY)) continue;
            if (GetTile(rndX, rndY).IsEnterable()) return WorldPos(rndX, rndY);
        }
    }

    // FIXME
    public Pos InitPos
    {
        get
        {
            for (int j = 1; j < Height - 1; j++)
            {
                Debug.Log("Height: " + j);
                for (int i = 1; i < Width - 1; i++)
                {
                    Debug.Log("Terrain: " + tileInfo[i, j]);
                    if (tileInfo[i, j] is Ground) return new Pos(i, j);
                }
            }
            return new Pos();
        }
    }

    // FIXME
    public IDirection InitDir => new South();
}