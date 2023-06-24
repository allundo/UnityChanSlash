using System;
using System.Linq;
using System.Collections.Generic;

public class MazeCreator
{
    public static readonly int PATH = (int)Terrain.Path;
    public static readonly int GROUND = (int)Terrain.Ground;
    public static readonly int WALL = (int)Terrain.Wall;
    public static readonly int DOOR = (int)Terrain.Door;

    public int[,] matrix { get; protected set; }

    public Terrain[,] Matrix { get; protected set; }

    private void SetTerrainData(int[,] matrix)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Matrix[x, y] = Util.ConvertTo<Terrain>(matrix[x, y]);
            }
        }
    }

    public int Width { get; private set; }
    public int Height { get; private set; }

    // 乱数生成用
    private Random rnd;

    public List<Pos> roomCenterPos { get; private set; } = new List<Pos>();

    public MazeCreator(int width = 49, int height = 49, Random rnd = null)
    {
        // Minimum size is 5 and even-number size is not available.
        if (width < 5 || height < 5 || width % 2 == 0 || height % 2 == 0) throw new ArgumentOutOfRangeException();

        this.Width = width;
        this.Height = height;

        this.Matrix = new Terrain[width, height];
        this.rnd = rnd ?? new Random();

        var matrix = CreateMaze(width, height);
        this.roomCenterPos = CreateRooms(matrix, width, height);
        SetTerrainData(matrix);

        this.matrix = matrix;
    }

    private int[,] CreateMaze(int width, int height)
    {
        List<Pos> startCells = new List<Pos>();
        int[,] matrix = new int[width, height];

        // Set WALLs to edges of maze.
        for (int y = 0; y < height; y++) matrix[0, y] = matrix[width - 1, y] = WALL;
        for (int x = 0; x < width; x++) matrix[x, 0] = matrix[x, height - 1] = WALL;

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                // Initialize inside edges of maze by PATHs.
                matrix[x, y] = PATH;

                if (x % 2 == 0 && y % 2 == 0)
                {
                    // Store grids position as candidate to start setting walls.
                    startCells.Add(new Pos(x, y));
                }
            }
        }

        startCells = startCells.Shuffle().ToList();

        while (startCells.Count > 0)
        {
            Stack<Pos> usedGrids = new Stack<Pos>();
            ExtendWall(matrix, startCells[0], usedGrids);

            startCells.Remove(usedGrids);
        }

        return matrix;
    }

    private bool TrySetWall(int[,] matrix, Pos pos, Pos dir, Stack<Pos> currentGrids)
    {
        var wallPos = pos + dir;
        var destPos = wallPos + dir;

        // Block if extending direction is already shut by WALL or next position is an current extending wall.
        if (matrix[wallPos.x, wallPos.y] == WALL || currentGrids.Contains(destPos)) return false;

        matrix[wallPos.x, wallPos.y] = WALL;
        return true;
    }

    private bool ExtendWall(int[,] matrix, Pos pos, Stack<Pos> currentGrids)
    {
        currentGrids.Push(pos);

        if (matrix[pos.x, pos.y] == WALL) return false;

        matrix[pos.x, pos.y] = WALL;

        // Shuffled list of extending direction candidates.
        Pos[] dirCandidates = new Pos[] { new Pos(0, -1), new Pos(0, 1), new Pos(-1, 0), new Pos(1, 0) }.Shuffle().ToArray();

        foreach (Pos dir in dirCandidates)
        {
            if (TrySetWall(matrix, pos, dir, currentGrids))
            {
                // Achieve to wall and stop extending if false.
                if (!ExtendWall(matrix, pos + dir * 2, currentGrids)) return false;
            }
        }

        // No direction to extend. Back to the previous position.
        return true;
    }

    private Pos[] GetPaths()
    {
        var list = new List<Pos>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (matrix[x, y] == PATH) list.Add(new Pos(x, y));
            }
        }

        return list.ToArray();
    }

    public Dictionary<Pos, IDirection> SearchDeadEnds()
    {
        var deadEndPos = new Dictionary<Pos, IDirection>();

        GetPaths().ForEach(pos =>
        {
            var list = new List<IDirection>();

            if (matrix[pos.x, pos.y - 1] != WALL) list.Add(Direction.north);
            if (matrix[pos.x, pos.y + 1] != WALL) list.Add(Direction.south);
            if (matrix[pos.x - 1, pos.y] != WALL) list.Add(Direction.west);
            if (matrix[pos.x + 1, pos.y] != WALL) list.Add(Direction.east);

            if (list.Count == 1) deadEndPos.Add(pos, list[0]);
        });

        return deadEndPos;
    }

    private List<Pos> CreateRooms(int[,] matrix, int width, int height)
    {
        int wHalf = (width + 1) / 2;
        int hHalf = (height + 1) / 2;

        int numOfDivide = (wHalf + hHalf) / 16;

        List<Pos> roomCenterPos = new List<Pos>();

        // Get HALF of the room left-top points.
        List<Pos> dividingRandomPos = GetDividingRandomPos(wHalf, hHalf, new Pos(0, 0), numOfDivide, 1);

        foreach (Pos pos in dividingRandomPos)
        {
            roomCenterPos.Add(MakeRoom(matrix, width, height, pos));
        }

        return roomCenterPos;
    }

    /// <summary>
    /// 特定の広さの領域を4分割して、それぞれの分割領域からランダム座標を取得 <br />
    /// numOfDivide で分割回数を指定し、再帰的に分割を行った領域から座標を取得する
    /// </summary>
    /// <param name="w">width of dividing region</param>
    /// <param name="h">height of dividing region</param>
    /// <param name="offset">left top of dividing region</param>
    /// <param name="numOfDivide">repeat times of dividing</param>
    /// <param name="trim">shrink size of dividing region</param>
    /// <returns></returns>
    private List<Pos> GetDividingRandomPos(int w, int h, Pos offset, int numOfDivide = 1, int trim = 0)
    {
        w -= trim;
        h -= trim;

        if (numOfDivide == 0)
        {
            return new List<Pos>(new Pos[] { GetRandomPos(w, h, offset) });
        }

        return GetDividingRandomPos(w / 2, h / 2, offset, numOfDivide - 1)
        .Concat(GetDividingRandomPos(w / 2, h / 2, offset.AddX(w / 2), numOfDivide - 1))
        .Concat(GetDividingRandomPos(w / 2, h / 2, offset.AddY(h / 2), numOfDivide - 1))
        .Concat(GetDividingRandomPos(w / 2, h / 2, offset.AddX(w / 2).AddY(h / 2), numOfDivide - 1)).ToList<Pos>();
    }
    private Pos GetRandomPos(int w, int h, Pos offset)
        => new Pos(offset.x + rnd.Next(0, w), offset.y + rnd.Next(0, h));

    /// <summary>
    /// Create a room into maze data matrix.
    /// </summary>
    /// <param name="matrix">Maze data matrix to edit</param>
    /// <param name="width">Width of the maze</param>
    /// <param name="height">Height of the maze</param>
    /// <param name="posHalf">HALF of the room left-top point.</param>
    /// <returns>Room center position</returns>
    private Pos MakeRoom(int[,] matrix, int width, int height, Pos posHalf)
    {
        // pos.x, pos.y, w, h must be even-number to adjust rooms to grid points.
        Pos pos = posHalf * 2;

        int w = rnd.Next(2, 4) * 2;
        int h = rnd.Next(2, 4) * 2;

        // Shrinks room size if exceeds region
        // Extends room size 1 block if touches outer wall
        if (pos.x + w > width - 1) w = width - 1 - pos.x;
        if (pos.y + h > height - 1) h = height - 1 - pos.y;

        // Set inner surface as ground
        for (int i = pos.x + 1; i < pos.x + w; i++)
        {
            for (int j = pos.y + 1; j < pos.y + h; j++)
            {
                matrix[i, j] = GROUND;
            }
        }

        // Set door as path on room edge
        for (int i = pos.x + 1; i < pos.x + w; i++)
        {
            if (matrix[i, pos.y] == PATH) matrix[i, pos.y] = DOOR;
            if (matrix[i, pos.y + h] == PATH) matrix[i, pos.y + h] = DOOR;
        }

        for (int j = pos.y + 1; j < pos.y + h; j++)
        {
            if (matrix[pos.x, j] == PATH) matrix[pos.x, j] = DOOR;
            if (matrix[pos.x + w, j] == PATH) matrix[pos.x + w, j] = DOOR;
        }

        // Room center position
        return pos + new Pos(w / 2, h / 2);
    }

#if UNITY_EDITOR
    public void DebugMatrix()
    {
        var str = new System.Text.StringBuilder();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                str.Append(matrix[x, y]);
            }
            str.Append("\n");
        }

        UnityEngine.Debug.Log(str);
    }
#endif
}
