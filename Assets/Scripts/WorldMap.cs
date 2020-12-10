using System;
using UnityEngine;

public class WorldMap
{
    public static readonly float TILE_UNIT = 2.5f;

    public Terrain[,] Matrix { get; protected set; }

    public Terrain GetTerrain(Pos pos) => GetTerrain(pos.x, pos.y);
    public Terrain GetTerrain(int x, int y) => IsOutWall(x, y) ? Terrain.Wall : Matrix[x, y];

    public bool IsOutWall(int x, int y) => x <= 0 || y <= 0 || x >= Width - 1 || y >= Height - 1;

    public int Width { get; protected set; } = 50;
    public int Height { get; protected set; } = 50;

    public WorldMap(int width, int height, Terrain[,] matrix = null)
    {
        this.Width = width;
        this.Height = height;

        Matrix = matrix == null ? new Terrain[width, height] : (Terrain[,])matrix.Clone();
    }

    public WorldMap(Dungeon dungeon) : this(dungeon.Width, dungeon.Height, dungeon.Matrix) { }

    public (float x, float z) WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public (float x, float z) WorldPos(int x, int y) => ((0.5f + x - Width * 0.5f) * TILE_UNIT, (-0.5f - y + Height * 0.5f) * TILE_UNIT);

    public Pos MapPos(Vector3 pos) =>
        new Pos(
            (int)Math.Round((Width - 1) * 0.5f + pos.x / TILE_UNIT, MidpointRounding.AwayFromZero),
            (int)Math.Round((Height - 1) * 0.5f - pos.z / TILE_UNIT, MidpointRounding.AwayFromZero)
        );

    // FIXME
    public (float x, float z) InitPos
    {
        get
        {
            for (int j = 1; j < Height - 1; j++)
            {
                Debug.Log("Height: " + j);
                for (int i = 1; i < Width - 1; i++)
                {
                    Debug.Log("Terrain: " + Matrix[i, j]);
                    if (Matrix[i, j] == Terrain.Ground) return WorldPos(i, j);
                }
            }
            return (0, 0);
        }
    }

    // FIXME
    public Direction InitDir => new South();
}

public enum Terrain
{
    None,
    Ground,
    Wall,
    Branch,
    VDoor,
    HDoor,
    TmpWall
}