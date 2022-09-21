using System;
using System.Linq;
using System.Collections.Generic;

public class MapManager
{
    public int floor { get; private set; } = 0;
    public int width { get; private set; }
    public int height { get; private set; }

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
    public List<Pos> roomCenterPos { get; private set; } = new List<Pos>();
    public List<Pos> pitTrapPos { get; private set; } = new List<Pos>();
    public List<Pos> fixedMessagePos { get; private set; } = new List<Pos>();

    /// <summary>
    /// Represents the start position and direction after going down a floor.
    /// </summary>
    public KeyValuePair<Pos, IDirection> stairsBottom { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);

    private bool IsUpstairsSet => !stairsBottom.Key.IsNull;

    /// <summary>
    /// Represents the start position and direction after going up a floor.
    /// </summary>
    public KeyValuePair<Pos, IDirection> stairsTop { get; private set; } = new KeyValuePair<Pos, IDirection>(new Pos(), null);

    private bool IsDownstairsSet => !stairsTop.Key.IsNull;

    public Terrain[,] matrix { get; protected set; }
    public Dir[,] dirMap { get; protected set; }

    public int[] GetMapData()
    {
        var ret = new int[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ret[x + y * width] = (int)matrix[x, y];
            }
        }
        return ret;
    }

    public int[] GetDirData()
    {
        var ret = new int[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ret[x + y * width] = (int)dirMap[x, y];
            }
        }
        return ret;
    }

#if UNITY_EDITOR
    public void DebugMatrix()
    {
        var str = new System.Text.StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                str.Append((int)matrix[x, y]);
            }
            str.Append("\n");
        }

        UnityEngine.Debug.Log(str);
    }

#endif

    public MapManager(int floor, int width = 49, int height = 49)
    {
        this.floor = floor;
        this.width = width;
        this.height = height;

        dirMap = new Dir[width, height];

        var maze = new MazeCreator(width, height, new Random())
            .CreateMaze()
            .SearchDeadEnds(new Pos(width - 2, height - 2))
            .CreateRooms();

        matrix = maze.Matrix.Clone() as Terrain[,];
        deadEndPos = new Dictionary<Pos, IDirection>(maze.deadEndPos);
        roomCenterPos = new List<Pos>(maze.roomCenterPos);

        CreateDirMap();

        SetDownStairs(floor);
        SetUpStairsOrStartDoor(floor);
        SetPitAndMessageBoards(floor);
    }

    public MapManager(int floor, DataStoreAgent.MapData mapData)
    {
        var matrix = mapData.mapMatrix;
        var dirMap = mapData.dirMap;

        this.floor = floor;
        this.width = mapData.mapSize;
        this.height = matrix.Length / width;
        this.roomCenterPos = mapData.roomCenterPos.ToList();
        this.deadEndPos = new Dictionary<Pos, IDirection>();
        this.stairsBottom = mapData.stairsBottom.Convert();
        this.stairsTop = mapData.stairsTop.Convert();

        this.matrix = new Terrain[width, height];
        this.dirMap = new Dir[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                this.matrix[i, j] = Util.ConvertTo<Terrain>(matrix[i + j * width]);
                this.dirMap[i, j] = Util.ConvertTo<Dir>(dirMap[i + j * width]);
            }
        }
    }

    public MapManager(int floor, int[] matrix, int width, Dictionary<Pos, IDirection> deadEndPos = null)
    {
        this.floor = floor;
        this.width = width;
        this.height = matrix.Length / width;
        this.roomCenterPos = new List<Pos>();

        this.matrix = new Terrain[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                this.matrix[i, j] = Util.ConvertTo<Terrain>(matrix[i + j * width]);
                if (this.matrix[i, j] == Terrain.RoomCenter)
                {
                    this.roomCenterPos.Add(new Pos(i, j));
                    this.matrix[i, j] = Terrain.Ground;
                }
            }
        }

        this.dirMap = new Dir[width, height];
        CreateDirMap();

        // deadEndPos is set by user
        if (deadEndPos != null)
        {
            this.deadEndPos = deadEndPos;
            SetDownStairs(floor);
            SetUpStairs(deadEndPos.Last().Key);
        }
        else
        {
            this.deadEndPos = new MatrixHandler(matrix, width).SearchDeadEnds();
            SetDownStairs(floor);
            SetUpStairsOrStartDoor(floor);
        }
    }

    public MapManager SetDownStairs(int floor)
    {
        this.floor = floor;
        return floor == GameInfo.Instance.LastFloor ? this : SetDownStairs();
    }
    private MapManager SetDownStairs() => SetDownStairs(deadEndPos.First().Key);
    private MapManager SetDownStairs(Pos pos) => SetDownStairs(pos, GetDownStairsDir);
    private MapManager SetDownStairs(Pos pos, IDirection dir) => SetDownStairs(pos, _ => dir);
    private MapManager SetDownStairs(Pos pos, Func<Pos, IDirection> DirGetter)
    {
        if (IsDownstairsSet) return this;

        IDirection dir = DirGetter(pos);

        matrix[pos.x, pos.y] = Terrain.DownStairs;
        dirMap[pos.x, pos.y] = dir.Enum;

        stairsTop = new KeyValuePair<Pos, IDirection>(dir.GetForward(pos), dir);
        matrix[stairsTop.Key.x, stairsTop.Key.y] = Terrain.Ground;

        deadEndPos.Remove(pos);

        return this;
    }

    protected IDirection GetDownStairsDir(Pos pos)
    {
        IDirection dir;
        deadEndPos.TryGetValue(pos, out dir);
        dir = dir ?? stairsBottom.Value?.Backward ?? Direction.south;

        return dir;
    }

    private MapManager SetStartDoor() => SetStartDoor(GetStartPos());
    private MapManager SetStartDoor(Pos pos) => SetStartDoor(pos, GetUpStairsDir);
    private MapManager SetStartDoor(Pos pos, IDirection dir) => SetStartDoor(pos, _ => dir);
    private MapManager SetStartDoor(Pos pos, Func<Pos, IDirection> DirGetter)
    {
        if (!stairsBottom.Key.IsNull) return this;

        IDirection dir = DirGetter(pos);

        stairsBottom = new KeyValuePair<Pos, IDirection>(pos, dir);

        // Don't set door in front of player start position
        Pos front = dir.GetForward(pos);
        matrix[front.x, front.y] = Terrain.Ground;

        Pos doorPos;
        if (IsAroundWall(doorPos = dir.GetLeft(pos)))
        {
            SetExitDoor(doorPos, dir.Right);
            SetFixedMessage(dir.GetBackward(pos), dir);
        }
        else if (IsAroundWall(doorPos = dir.GetRight(pos)))
        {
            SetExitDoor(doorPos, dir.Left);
            SetFixedMessage(dir.GetBackward(pos), dir);
        }
        else if (IsAroundWall(doorPos = dir.GetBackward(pos)))
        {
            SetExitDoor(doorPos, dir);
            SetFixedMessage(dir.GetLeft(pos), dir.Right);
        }
        else
        {
#if UNITY_EDITOR
            DebugMatrix();
#endif
            throw new Exception("Position: (" + pos.x + ", " + pos.y + ") isn't suitable to start.");
        }

        deadEndPos.Remove(pos);

        return this;
    }
    protected void SetExitDoor(Pos pos, IDirection doorDir)
    {
        Pos leftPos = doorDir.GetLeft(pos);
        Pos rightPos = doorDir.GetRight(pos);

        matrix[pos.x, pos.y] = Terrain.ExitDoor;
        matrix[leftPos.x, leftPos.y] = matrix[rightPos.x, rightPos.y] = Terrain.Pillar;
        dirMap[pos.x, pos.y] = doorDir.Enum;
        dirMap[leftPos.x, leftPos.y] |= doorDir.Right.Enum;
        dirMap[rightPos.x, rightPos.y] |= doorDir.Left.Enum;
    }

    protected void SetFixedMessage(Pos pos, IDirection dir)
    {
        fixedMessagePos.Add(pos);
        SetMessageBoard(pos, dir);
    }

    protected void SetExitDoorMessage(Pos pos, IDirection dir)
    {
        // Insert the position as first element
        fixedMessagePos = new Pos[] { pos }.Concat(fixedMessagePos).ToList(); ;
        SetMessageBoard(pos, dir);
    }

    protected void SetMessageBoard(Pos pos, IDirection boardDir)
    {
        dirMap[pos.x, pos.y] = boardDir.Enum;
        matrix[pos.x, pos.y] = Terrain.MessageWall;
    }

    protected bool IsAroundWall(Pos pos) => IsAroundWall(pos.x, pos.y);
    protected bool IsAroundWall(int x, int y) => x == 0 || x == width - 1 || y == 0 || y == height - 1;
    protected bool IsNextToAroundWall(Pos pos) => IsNextToAroundWall(pos.x, pos.y);
    protected bool IsNextToAroundWall(int x, int y) => x == 1 || x == width - 2 || y == 1 || y == height - 2;

    public MapManager SetUpStairsOrStartDoor(int floor)
    {
        this.floor = floor;
        return floor == 1 ? SetStartDoor() : SetUpStairs();
    }
    private MapManager SetUpStairs() => SetUpStairs(GetUpStairsPos());
    public MapManager SetUpStairs(Pos pos) => SetUpStairs(pos, GetUpStairsDir);
    private MapManager SetUpStairs(Pos pos, IDirection dir) => SetUpStairs(pos, _ => dir);
    private MapManager SetUpStairs(Pos pos, Func<Pos, IDirection> DirGetter)
    {
        IDirection dir = DirGetter(pos);

        if (this.floor == 1) stairsBottom = new KeyValuePair<Pos, IDirection>(pos, dir);

        if (IsUpstairsSet) return this;

        matrix[pos.x, pos.y] = Terrain.UpStairs;
        dirMap[pos.x, pos.y] = dir.Enum;

        stairsBottom = new KeyValuePair<Pos, IDirection>(dir.GetForward(pos), dir);
        matrix[stairsBottom.Key.x, stairsBottom.Key.y] = Terrain.Ground;

        deadEndPos.Remove(pos);

        return this;
    }

    private Pos GetStartPos()
    {
        Pos startPos = deadEndPos.Keys.Where(pos => IsNextToAroundWall(pos)).LastOrDefault();

        if (!startPos.IsNull) return startPos;

        var posList = new Pos[] { new Pos(1, 1), new Pos(width - 2, 1), new Pos(1, height - 2), new Pos(width - 2, height - 2) }
            .OrderByDescending(pos => stairsTop.Key.Distance(pos));

        foreach (Pos pos in posList)
        {
            var dir = GetPassableDir(pos);
            if (dir != null)
            {
                deadEndPos[pos] = dir;
                return pos;
            }
        }

        throw new InvalidOperationException("Failed to create ExitDoor.");
    }

    private IDirection GetPassableDir(Pos pos)
    {
        if (IsPassable(pos.DecX())) return Direction.north;
        if (IsPassable(pos.IncX())) return Direction.east;
        if (IsPassable(pos.IncY())) return Direction.south;
        if (IsPassable(pos.DecX())) return Direction.west;
        return null;
    }

    private bool IsPassable(Pos pos) => matrix[pos.x, pos.y] == Terrain.Ground || matrix[pos.x, pos.y] == Terrain.Path;

    private Pos GetUpStairsPos()
    {
        if (stairsTop.Key.IsNull) return deadEndPos.Last().Key;

        float further = 0f;

        Pos upStairsPos = new Pos();

        deadEndPos.ForEach(kv =>
        {
            float distance = stairsTop.Key.Distance(kv.Key);
            if (distance > further)
            {
                upStairsPos = kv.Key;
                further = distance;
                if (further > Math.Min(width, height)) return false;
            }
            return true;
        });

        if (upStairsPos.IsNull)
        {
            upStairsPos = roomCenterPos.Last();
            roomCenterPos.Remove(upStairsPos);
        }

        return upStairsPos;
    }

    protected IDirection GetUpStairsDir(Pos pos)
    {
        IDirection dir;
        deadEndPos.TryGetValue(pos, out dir);
        dir = dir ?? stairsTop.Value?.Backward ?? Direction.north;

        return dir;
    }

    public MapManager SetStairs(int floor) => SetDownStairs(floor).SetUpStairsOrStartDoor(floor);
    public MapManager InitMap(int floor) => SetStairs(floor).SetPitAndMessageBoards(floor);

    private MapManager SetPitAndMessageBoards(int floor)
    {
        var pitCandidates = new List<Pos>();
        var boardCandidates = new Dictionary<Pos, IDirection>();

        for (int y = 1; y < height; y++)
        {
            for (int x = 1; x < width; x++)
            {
                Pos pos = new Pos(x, y);

                // Allow placing on ground or path
                if (matrix[x, y] != Terrain.Ground && matrix[x, y] != Terrain.Path) continue;

                if ((x + y) % 2 == 0)
                {
                    // Forbid placing boards on around walls of dead end position
                    if (!deadEndPos.ContainsKey(pos)) SetBoardCandidate(boardCandidates, x, y);
                }
                else
                {
                    pitCandidates.Add(pos);
                }
            }
        }

        // Forbid placing pit traps in front of items and stairs
        deadEndPos.ForEach(kv => { pitCandidates.Remove(kv.Value.GetForward(kv.Key)); });
        pitCandidates.Remove(stairsBottom.Key);
        pitCandidates.Remove(stairsTop.Key);

        FloorMessagesSource src = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);

        int numOfPits = Math.Min(floor * floor * 4, width * height / 10);
        int numOfFixedMessages = src.fixedMessages.Length;
        int numOfRandomMessages = src.randomMessages.Length;

        // Set pit traps
        for (int i = 0; i < numOfPits && pitCandidates.Count > 0; i++)
        {
            Pos pos = pitCandidates.GetRandom();
            matrix[pos.x, pos.y] = Terrain.Pit;
            pitCandidates.Remove(pos);
            pitTrapPos.Add(pos);
        }

        if (floor == 1)
        {
            // The first message is used for Exit Door
            numOfFixedMessages--;

            pitTrapPos.ForEach(pos =>
            {
                // Fixed messages are used for Pit Trap Attentions
                numOfFixedMessages--;

                bool isSet = false;
                // Try setting pit attention message to north-west, north-east, east-south, south-west wall of each pit position.
                new Pos[] { pos.DecX().DecY(), pos.IncX().DecY(), pos.IncX().IncY(), pos.DecX().IncY() }.ForEach(dest =>
                {
                    if (!isSet) isSet = TrySetPitAttention(boardCandidates, pos, dest);

                    IDirection dir;
                    if (boardCandidates.TryGetValue(dest, out dir)) boardCandidates[dest] = BoardDirection(pos, dest, dir).Backward;
                });

                // Makes up for decrease of Pit Trap Attentions
                if (!isSet) numOfRandomMessages++;
            });
        }

        for (int i = 0; i < numOfFixedMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetFixedMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
        }

        for (int i = 0; i < numOfRandomMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetMessageBoard(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
        }

        return this;
    }

    private bool TrySetPitAttention(Dictionary<Pos, IDirection> boardCandidates, Pos pitPos, Pos dest)
    {
        IDirection dir;

        if (boardCandidates.TryGetValue(dest, out dir))
        {
            SetFixedMessage(dest, BoardDirection(pitPos, dest, dir));
            boardCandidates.Remove(dest);
            return true;
        }

        return false;
    }

    private IDirection BoardDirection(Pos pitPos, Pos boardPos, IDirection boardDir)
    {
        Pos vecToReadPos = boardDir.GetForward(boardPos) - pitPos;
        switch (Math.Abs(vecToReadPos.x) + Math.Abs(vecToReadPos.y))
        {
            case 1: return boardDir;
            case 3: return boardDir.Backward;
            default: throw new ArgumentException("Invalid pair of pit and board position.");
        }
    }

    private bool SetBoardCandidate(Dictionary<Pos, IDirection> candidates, int x, int y)
    {
        if (matrix[x, y + 1] == Terrain.Wall)
        {
            candidates[new Pos(x, y + 1)] = Direction.north;
            return true;
        }

        if (matrix[x - 1, y] == Terrain.Wall)
        {
            candidates[new Pos(x - 1, y)] = Direction.east;
            return true;
        }

        if (matrix[x, y - 1] == Terrain.Wall)
        {
            candidates[new Pos(x, y - 1)] = Direction.south;
            return true;
        }

        if (matrix[x + 1, y] == Terrain.Wall)
        {
            candidates[new Pos(x + 1, y)] = Direction.west;
            return true;
        }

        return false;
    }

    private void CreateDirMap()
    {
        // Set direction to door and wall
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Skip pillar or gate
                if (x % 2 == 0 && y % 2 == 0) continue;

                if (matrix[x, y] == Terrain.MessageWall || matrix[x, y] == Terrain.ExitDoor || matrix[x, y] == Terrain.Box)
                {
                    dirMap[x, y] = GetValidDir(x, y);
                    continue;
                }

                dirMap[x, y] = GetWallDir(x, y);

            }
        }

        // Set pillar or gate
        for (int y = 0; y < height; y += 2)
        {
            for (int x = 0; x < width; x += 2)
            {
                if (matrix[x, y] == Terrain.MessagePillar || matrix[x, y] == Terrain.Box)
                {
                    dirMap[x, y] = GetValidDir(x, y);
                    continue;
                }

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
                // Set pillar as corner wall
                matrix[x, y] = Terrain.Pillar;
                dirMap[x, y] = doorDir;
            }
        }
    }

    private Dir GetDir(int x, int y, Terrain type)
    {
        Dir dir = Dir.NONE;

        Terrain up = (y - 1 < 0) ? Terrain.Ground : matrix[x, y - 1];
        Terrain left = (x - 1 < 0) ? Terrain.Ground : matrix[x - 1, y];
        Terrain down = (y + 1 >= height) ? Terrain.Ground : matrix[x, y + 1];
        Terrain right = (x + 1 >= width) ? Terrain.Ground : matrix[x + 1, y];

        if (up == type) dir |= Dir.N;
        if (down == type) dir |= Dir.S;
        if (left == type) dir |= Dir.W;
        if (right == type) dir |= Dir.E;

        return dir;
    }
    public Dir GetDoorDir(int x, int y) => GetDir(x, y, Terrain.Door) | GetDir(x, y, Terrain.ExitDoor);
    private Dir GetWallDir(int x, int y) => GetDir(x, y, Terrain.Wall) | GetDir(x, y, Terrain.MessageWall);
    public Dir GetPillarDir(int x, int y) => GetDir(x, y, Terrain.Pillar) | GetDir(x, y, Terrain.MessagePillar);

    public Dir GetValidDir(int x, int y)
    {
        if (y > 0 && IsEnterable(matrix[x, y - 1])) return Dir.N;
        if (x > 0 && IsEnterable(matrix[x - 1, y])) return Dir.W;
        if (y < height - 2 && IsEnterable(matrix[x, y + 1])) return Dir.S;
        if (x < width - 2 && IsEnterable(matrix[x + 1, y])) return Dir.E;

        return Dir.NONE;
    }

    private bool IsEnterable(Terrain terrain) => terrain == Terrain.Ground || terrain == Terrain.Path;

    public class MazeCreator
    {
        // 方向
        private enum MoveDir
        {
            None = 0,
            Up = 1,
            Right = 2,
            Down = 3,
            Left = 4
        }

        // 2次元配列の迷路情報
        public Terrain[,] Matrix { get; protected set; }
        public int Width { get; }
        public int Height { get; }

        // 乱数生成用
        private Random rnd;
        // 現在拡張中の壁情報を保持
        private Stack<Pos> CurrentWallCells;
        //private Stack<int> CurrentWallIndex;
        // 壁の拡張を行う開始セルの情報
        private List<Pos> StartCells;

        public Dictionary<Pos, IDirection> deadEndPos { get; private set; }
            = new Dictionary<Pos, IDirection>();

        public List<Pos> roomCenterPos { get; private set; } = new List<Pos>();

        // コンストラクタ
        public MazeCreator(int width = 49, int height = 49, Random rnd = null)
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
            this.rnd = rnd ?? new Random();
        }

        public MazeCreator CreateMaze()
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
                var index = rnd.Next(StartCells.Count);
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

            return this;
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
                int dirIndex = rnd.Next(directions.Count);

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
            => CurrentWallCells.Contains(new Pos(x, y));

        public MazeCreator SearchDeadEnds(Pos startPos)
        {
            if (!IsPassable(Matrix[startPos.x, startPos.y]))
            {
                throw new ArgumentException("Position: (" + startPos.x + ", " + startPos.y + ") is not a Path but ... " + Matrix[startPos.x, startPos.y]);
            }

            var mat = (Terrain[,])Matrix.Clone();
            var stack = new Stack<(IDirection, Pos)>();

            stack.Push((new North(), startPos));

            while (stack.Count > 0)
            {
                (IDirection dir, Pos pos) = stack.Pop();

                Pos forward = dir.GetForward(pos);
                Pos right = dir.GetRight(pos);
                Pos left = dir.GetLeft(pos);

                Terrain ft = IsOutWall(forward) ? Terrain.Wall : mat[forward.x, forward.y];
                Terrain lt = IsOutWall(left) ? Terrain.Wall : mat[left.x, left.y];
                Terrain rt = IsOutWall(right) ? Terrain.Wall : mat[right.x, right.y];

                int count = stack.Count;

                if (IsPassable(ft)) stack.Push((dir, forward));
                if (IsPassable(lt)) stack.Push((dir.Left, left));
                if (IsPassable(rt)) stack.Push((dir.Right, right));

                if (stack.Count == count) deadEndPos[pos] = dir.Backward;

                mat[pos.x, pos.y] = Terrain.Wall;
            }

            return this;
        }

        public bool IsPassable(Terrain terrain) => terrain == Terrain.Path || terrain == Terrain.Ground;
        public bool IsOutWall(Pos pos) => IsOutWall(pos.x, pos.y);
        public bool IsOutWall(int x, int y) => x <= 0 || y <= 0 || x >= Width - 1 || y >= Height - 1;

        public MazeCreator CreateRooms()
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

            return this;
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
        private Pos GetRandomPos(int w, int h, Pos offset, int trim = 0)
            => new Pos(offset.x + rnd.Next(0, w - trim), offset.y + rnd.Next(0, h - trim));

        private void MakeRoom(Pos posHalf)
        {
            int w = rnd.Next(2, 4) * 2;
            int h = rnd.Next(2, 4) * 2;

            Pos pos = posHalf * 2;

            // Shrinks room size if exceeds region
            // Extends room size 1 block if touches outer wall
            if (pos.x + w > Width - 1) w = Width - 1 - pos.x;
            if (pos.y + h > Height - 1) h = Height - 1 - pos.y;

            roomCenterPos.Add(pos + new Pos(w / 2, h / 2));

            // Set inner surface as ground
            for (int i = pos.x + 1; i < pos.x + w; i++)
            {
                for (int j = pos.y + 1; j < pos.y + h; j++)
                {
                    deadEndPos.Remove(new Pos(i, j));
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
    }
}