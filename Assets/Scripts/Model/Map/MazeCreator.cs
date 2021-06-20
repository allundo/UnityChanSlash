using System;
using System.Linq;
using System.Collections.Generic;


public class MazeCreator
{
    // セル情報
    private struct Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    // 方向
    private enum MoveDir
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4
    }

    public Terrain[,] Matrix { get; protected set; }

    // 2次元配列の迷路情報
    public int[,] Maze { get; private set; }
    public int Width { get; }
    public int Height { get; }

    // 乱数生成用
    private Random Random;
    // 現在拡張中の壁情報を保持
    private Stack<Pos> CurrentWallCells;
    //private Stack<int> CurrentWallIndex;
    // 壁の拡張を行う開始セルの情報
    private List<Pos> StartCells;

    // コンストラクタ
    public MazeCreator(int width = 49, int height = 49)
    {
        // 5未満のサイズや偶数では生成できない
        if (width < 5 || height < 5) throw new ArgumentOutOfRangeException();
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        // 迷路情報を初期化
        this.Width = width;
        this.Height = height;
        this.Matrix = new Terrain[width, height];

        StartCells = new List<Pos>();
        CurrentWallCells = new Stack<Pos>();
        this.Random = new Random();
    }

    public Terrain[,] CreateMaze()
    {
        // 各マスの初期設定を行う
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                // 外周のみ壁にしておき、開始候補として保持
                if (x == 0 || y == 0 || x == this.Width - 1 || y == this.Height - 1)
                {
                    this.Matrix[x, y] = Terrain.Wall;
                }
                else
                {
                    this.Matrix[x, y] = Terrain.Path;
                    // 外周ではない偶数座標を壁伸ばし開始点にしておく
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        // 開始候補座標
                        StartCells.Add(new Pos(x, y));
                    }
                }
            }
        }

        // 壁が拡張できなくなるまでループ
        while (StartCells.Count > 0)
        {
            // ランダムに開始セルを取得し、開始候補から削除
            var index = Random.Next(StartCells.Count);
            var cell = StartCells[index];
            StartCells.RemoveAt(index);
            var x = cell.x;
            var y = cell.y;

            // すでに壁の場合は何もしない
            if (this.Matrix[x, y] == Terrain.Path)
            {
                // 拡張中の壁情報を初期化
                CurrentWallCells.Clear();
                ExtendWall(x, y);
            }
        }

        (Direction stairDir, Pos stairPos) = SearchDeadEnd(new Pos(Width - 2, Height - 2));
        this.stairDir = stairDir;

        CreateRooms();

        SetStair(stairPos, stairDir);
        return this.Matrix;
    }

    // 指定座標から壁を生成拡張する
    private void ExtendWall(int x, int y)
    {
        // 伸ばすことができる方向(1マス先が通路で2マス先まで範囲内)
        // 2マス先が壁で自分自身の場合、伸ばせない
        var directions = new List<MoveDir>();
        if (this.Matrix[x, y - 1] == Terrain.Path && !IsCurrentWall(x, y - 2))
            directions.Add(MoveDir.Up);
        if (this.Matrix[x + 1, y] == Terrain.Path && !IsCurrentWall(x + 2, y))
            directions.Add(MoveDir.Right);
        if (this.Matrix[x, y + 1] == Terrain.Path && !IsCurrentWall(x, y + 2))
            directions.Add(MoveDir.Down);
        if (this.Matrix[x - 1, y] == Terrain.Path && !IsCurrentWall(x - 2, y))
            directions.Add(MoveDir.Left);

        // ランダムに伸ばす(2マス)
        if (directions.Count > 0)
        {
            // 壁を作成(この地点から壁を伸ばす)
            SetWall(x, y);

            // 伸ばす先が通路の場合は拡張を続ける
            var isPath = false;
            int dirIndex = Random.Next(directions.Count);

            switch (directions[dirIndex])
            {
                case MoveDir.Up:
                    isPath = (this.Matrix[x, y - 2] == Terrain.Path);
                    SetWall(x, --y);
                    SetWall(x, --y);
                    break;
                case MoveDir.Right:
                    isPath = (this.Matrix[x + 2, y] == Terrain.Path);
                    SetWall(++x, y);
                    SetWall(++x, y);
                    break;
                case MoveDir.Down:
                    isPath = (this.Matrix[x, y + 2] == Terrain.Path);
                    SetWall(x, ++y);
                    SetWall(x, ++y);
                    break;
                case MoveDir.Left:
                    isPath = (this.Matrix[x - 2, y] == Terrain.Path);
                    SetWall(--x, y);
                    SetWall(--x, y);
                    break;
            }
            if (isPath)
            {
                // 既存の壁に接続できていない場合は拡張続行
                ExtendWall(x, y);
            }
        }
        else
        {
            // すべて現在拡張中の壁にぶつかる場合、バックして再開
            Pos beforeCell = CurrentWallCells.Pop();
            ExtendWall(beforeCell.x, beforeCell.y);
        }
    }

    // 壁を拡張する
    private void SetWall(int x, int y)
    {
        this.Matrix[x, y] = Terrain.Wall;
        if (x % 2 == 0 && y % 2 == 0)
        {
            CurrentWallCells.Push(new Pos(x, y));
        }
    }

    // 拡張中の座標かどうか判定
    private bool IsCurrentWall(int x, int y)
    {
        return CurrentWallCells.Contains(new Pos(x, y));
    }

    private void CreateRooms()
    {
        int wHalf = (Width + 1) / 2;
        int hHalf = (Height + 1) / 2;

        int numOfDivide = (wHalf + hHalf) / 16;
        int numOfRandom = (wHalf + hHalf) / 10;

        Console.WriteLine("wHalf: " + wHalf + ", hHalf: " + hHalf + ", numOfDivide: " + numOfDivide + ", numOfRandom: " + numOfRandom);

        List<Pos> dividingRandomPos = GetDividingRandomPos(wHalf, hHalf, new Pos(0, 0), numOfDivide, 1);

        foreach (Pos pos in dividingRandomPos)
        {
            MakeRoom(pos);
        }

        for (int i = 0; i < numOfRandom; i++)
        {
            // MakeRoom(GetRandomPos(wHalf, hHalf, new Pos(0, 0), 1));
        }
    }

    private (Direction, Pos) SearchDeadEnd(Pos startPos)
    {
        if (!IsPathable(Matrix[startPos.x, startPos.y]))
        {
            throw new ArgumentException("Position: (" + startPos.x + ", " + startPos.y + ") is not a Path but ... " + Matrix[startPos.x, startPos.y]);
        }

        var mat = (Terrain[,])Matrix.Clone();
        var stack = new Stack<(Direction, Pos)>();

        stack.Push((new North(), startPos));

        while (stack.Count > 0)
        {
            (Direction dir, Pos pos) = stack.Pop();

            Pos forward = dir.GetForward(pos);
            Pos right = dir.GetRight(pos);
            Pos left = dir.GetLeft(pos);

            Terrain ft = IsOutWall(forward) ? Terrain.Wall : mat[forward.x, forward.y];
            Terrain lt = IsOutWall(left) ? Terrain.Wall : mat[left.x, left.y];
            Terrain rt = IsOutWall(right) ? Terrain.Wall : mat[right.x, right.y];

            int count = stack.Count;

            if (IsPathable(ft)) stack.Push((dir, forward));
            if (IsPathable(lt)) stack.Push((dir.Left, left));
            if (IsPathable(rt)) stack.Push((dir.Right, right));

            if (stack.Count == count) return (dir.Backward, pos);

            mat[pos.x, pos.y] = Terrain.Wall;
        }

        throw new Exception("Dead end not found");
    }

    public bool IsPathable(Terrain terrain) => terrain == Terrain.Path || terrain == Terrain.Ground;
    public bool IsOutWall(Pos pos) => IsOutWall(pos.x, pos.y);
    public bool IsOutWall(int x, int y) => x <= 0 || y <= 0 || x >= Width - 1 || y >= Height - 1;

    private Pos GetRandomPos(int w, int h, Pos offset, int trim = 0)
    {
        Random rnd = this.Random;
        return new Pos(offset.x + rnd.Next(0, w - trim), offset.y + rnd.Next(0, h - trim));
    }

    /// <summary>
    /// 特定の広さの領域を4分割して、それぞれの分割領域からランダム座標を取得
    /// numOfDivide で分割回数を指定し、再帰的に分割を行った領域から座標を取得する
    /// </summary>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="offset"></param>
    /// <param name="numOfDivide"></param>
    /// <param name="trim"></param>
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

    private void SetStair(Pos pos, Direction dir)
    {
        if (IsOutWall(pos)) throw new ArgumentException("Position: (" + pos.x + ", " + pos.y + ") is out of range");

        SetTerrain(Terrain.Stair, pos);

        Pos left = dir.GetLeft(pos);
        Pos right = dir.GetRight(pos);
        Pos back = dir.GetBackward(pos);
        Pos vecBack = back - pos;

        SetTerrain(Terrain.Wall, left, right, back, left + vecBack, right + vecBack);
    }

    private void SetTerrain(Terrain terrain, params Pos[] positions)
    {
        foreach (Pos pos in positions)
        {
            this.Matrix[pos.x, pos.y] = terrain;
        }
    }

    private void MakeRoom(Pos posHalf)
    {
        Random rnd = this.Random;
        int w = rnd.Next(2, 4) * 2;
        int h = rnd.Next(2, 4) * 2;

        Pos pos = posHalf * 2;

        // Shrinks room size if exceeds region
        // Extends room size 1 block if touches outer wall
        if (pos.x + w > Width - 1) w = Width - 1 - pos.x;
        if (pos.y + h > Height - 1) h = Height - 1 - pos.y;

        // Set inner surface as ground
        for (int i = pos.x + 1; i < pos.x + w; i++)
        {
            for (int j = pos.y + 1; j < pos.y + h; j++)
            {
                Matrix[i, j] = Terrain.Ground;
            }
        }

        // Set door as path on room edge
        for (int i = pos.x + 1; i < pos.x + w; i++)
        {
            SetDoor(i, pos.y);
            SetDoor(i, pos.y + h);
        }
        for (int j = pos.y + 1; j < pos.y + h; j++)
        {
            SetDoor(pos.x, j);
            SetDoor(pos.x + w, j);
        }
    }

    private void SetDoor(int x, int y)
    {
        if (Matrix[x, y] == Terrain.Path)
        {
            Matrix[x, y] = Terrain.Door;
        }
    }

    // デバッグ用処理
    public static void DebugPrint(Terrain[,] matrix)
    {
        Console.WriteLine($"Width: {matrix.GetLength(0)}");
        Console.WriteLine($"Height: {matrix.GetLength(1)}");
        for (int y = 0; y < matrix.GetLength(1); y++)
        {
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                switch (matrix[x, y])
                {
                    case Terrain.Wall:
                        Console.Write("#");
                        break;

                    case Terrain.Pall:
                        Console.Write("P");
                        break;

                    case Terrain.Door:
                        Console.Write("+");
                        break;

                    case Terrain.Ground:
                        Console.Write(" ");
                        break;

                    default:
                        Console.Write(".");
                        break;
                }
            }
            Console.WriteLine();
        }
    }

    public Dir[,] GetDirMap()
    {
        var dirMap = new Dir[Width, Height];

        // Set direction to door and wall
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Skip pall or gate
                if (x % 2 == 0 && y % 2 == 0) continue;

                dirMap[x, y] = GetWallDir(x, y);
            }
        }

        // Set pall or gate
        for (int y = 0; y < Height; y += 2)
        {
            for (int x = 0; x < Width; x += 2)
            {

                Dir doorDir = GetDoorDir(x, y);

                // Leave it as is if strait wall or room ground
                if (doorDir == Dir.NONE)
                {
                    Dir wallDir = GetWallDir(x, y);
                    if (wallDir == Dir.NS || wallDir == Dir.EW || wallDir == Dir.NONE)
                    {
                        dirMap[x, y] = wallDir;
                        continue;
                    }
                }

                // Set gate as wall next to door
                // Set pall as corner wall
                Matrix[x, y] = Terrain.Pall;
                dirMap[x, y] = doorDir;
            }
        }
        return dirMap;
    }

    private Dir GetDir(int x, int y, Terrain type)
    {
        Dir dir = Dir.NONE;

        Terrain up = (y - 1 < 0) ? Terrain.Ground : Matrix[x, y - 1];
        Terrain left = (x - 1 < 0) ? Terrain.Ground : Matrix[x - 1, y];
        Terrain down = (y + 1 >= Height) ? Terrain.Ground : Matrix[x, y + 1];
        Terrain right = (x + 1 >= Width) ? Terrain.Ground : Matrix[x + 1, y];

        if (up == type) dir |= Dir.N;
        if (down == type) dir |= Dir.S;
        if (left == type) dir |= Dir.W;
        if (right == type) dir |= Dir.E;

        return dir;
    }
    private Dir GetDoorDir(int x, int y)
    {
        return GetDir(x, y, Terrain.Door);
    }
    private Dir GetWallDir(int x, int y)
    {
        return GetDir(x, y, Terrain.Wall);
    }

    public Dir GetPallDir(int x, int y)
    {
        return GetDir(x, y, Terrain.Pall);
    }

    public Direction stairDir;
}