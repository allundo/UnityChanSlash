using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldMap
{
    public static readonly float TILE_UNIT = 2.5f;

    public Tile[,] Matrix { get; protected set; }
    public Tile GetTile(Vector3 pos) => GetTile(MapPos(pos));
    public Tile GetTile(Pos pos) => GetTile(pos.x, pos.y);
    public Tile GetTile(int x, int y) => IsOutWall(x, y) ? new Wall() : Matrix[x, y];

    public bool IsOutWall(int x, int y) => x <= 0 || y <= 0 || x >= Width - 1 || y >= Height - 1;

    public int Width { get; protected set; } = 49;
    public int Height { get; protected set; } = 49;

    public WorldMap(MazeCreator maze) : this(maze.Matrix) { }
    public WorldMap(Terrain[,] matrix) : this(matrix.GetLength(0), matrix.GetLength(1))
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                switch (matrix[i, j])
                {
                    case Terrain.Wall:
                        Matrix[i, j] = new Wall();
                        break;

                    case Terrain.Door:
                        Matrix[i, j] = new Door();
                        break;

                    case Terrain.Stair:
                        Matrix[i, j] = new Stair();
                        break;

                    default:
                        Matrix[i, j] = new Ground();
                        break;
                }
            }
        }
    }

    public WorldMap(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        Matrix = new Tile[width, height];
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
    public bool IsTileViewOpen(int x, int y) => IsOutWall(x, y) ? false : Matrix[x, y].IsViewOpen;
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
                    Debug.Log("Terrain: " + Matrix[i, j]);
                    if (Matrix[i, j] is Ground) return new Pos(i, j);
                }
            }
            return new Pos();
        }
    }

    // FIXME
    public Direction InitDir => new South();
}