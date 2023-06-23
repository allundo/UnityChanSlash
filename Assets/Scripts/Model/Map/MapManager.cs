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
    public List<Pos> bloodMessagePos { get; private set; } = new List<Pos>();

    public Pos doorPos { get; private set; }

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

        var maze = new MazeCreator(width, height, new Random());

        matrix = maze.Matrix.Clone() as Terrain[,];
        deadEndPos = new Dictionary<Pos, IDirection>(maze.SearchDeadEnds());
        roomCenterPos = new List<Pos>(maze.roomCenterPos);

        CreateDirMap();

        SetDownStairs(floor);
        SetUpStairsOrStartDoor(floor);

        deadEndPos = deadEndPos.Shuffle();

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

    public MapManager(
        int floor,
        int[] matrix,
        int width,
        Dictionary<Pos, IDirection> deadEndPos = null,
        Dictionary<Pos, IDirection> fixedMessagePos = null,
        Dictionary<Pos, IDirection> bloodMessagePos = null
    )
    {
        this.floor = floor;
        this.width = width;
        this.height = matrix.Length / width;
        this.roomCenterPos = new List<Pos>();

        this.matrix = new Terrain[width, height];

        var fixedMessageBuffer = new List<Pos>();
        var bloodMessageBuffer = new List<Pos>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                this.matrix[i, j] = Util.ConvertTo<Terrain>(matrix[i + j * width]);

                switch (this.matrix[i, j])
                {
                    case Terrain.RoomCenter:
                        this.roomCenterPos.Add(new Pos(i, j));
                        this.matrix[i, j] = Terrain.Ground;
                        continue;

                    case Terrain.MessageWall:
                    case Terrain.MessagePillar:
                        fixedMessageBuffer.Add(new Pos(i, j));
                        continue;

                    case Terrain.BloodMessageWall:
                    case Terrain.BloodMessagePillar:
                        bloodMessageBuffer.Add(new Pos(i, j));
                        continue;
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
            this.deadEndPos = this.deadEndPos.Shuffle();
        }

        FloorMessagesSource src = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);
        int numOfFixedMessage = src.fixedMessages.Length;
        int numOfBloodMessage = src.bloodMessages.Length;

        fixedMessagePos?.ForEach(kv =>
        {
            if (numOfFixedMessage-- > 0) SetFixedMessage(kv.Key, kv.Value);
        });

        for (int i = 0; i < fixedMessageBuffer.Count && i < numOfFixedMessage; i++)
        {
            this.fixedMessagePos.Add(fixedMessageBuffer[i]);
        }

        bloodMessagePos?.ForEach(kv =>
        {
            if (numOfBloodMessage-- > 0) SetBloodMessage(kv.Key, kv.Value);
        });

        int bloodCount = bloodMessageBuffer.Count;
        int bloodOverCount = bloodCount - Math.Max(0, numOfBloodMessage);
        if (bloodOverCount > 0)
        {
            var bloodOver = bloodMessageBuffer.GetRange(bloodCount - bloodOverCount, bloodOverCount);
            bloodMessageBuffer.RemoveRange(bloodCount - bloodOverCount, bloodOverCount);

            // Convert surplus blood messages to random messages.
            bloodOver.ForEach(pos => this.matrix[pos.x, pos.y] =
                this.matrix[pos.x, pos.y] == Terrain.BloodMessageWall
                    ? Terrain.MessageWall
                    : Terrain.MessagePillar
            );
        }

        this.bloodMessagePos.AddRange(bloodMessageBuffer);
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

    protected void SetFixedMessage(Pos pos, IDirection boardDir)
    {
        fixedMessagePos.Add(pos);
        SetMessageBoard(pos, boardDir, Terrain.MessageWall);
    }

    protected void SetBloodMessage(Pos pos, IDirection boardDir)
    {
        bloodMessagePos.Add(pos);
        SetMessageBoard(pos, boardDir, Terrain.BloodMessageWall);
    }

    protected void SetMessageBoard(Pos pos, IDirection boardDir, Terrain type = Terrain.MessageWall)
    {
        dirMap[pos.x, pos.y] = boardDir.Enum;
        switch (matrix[pos.x, pos.y])
        {
            case Terrain.Wall:
                matrix[pos.x, pos.y] = type;
                break;
            case Terrain.Pillar:
                // Convert terrain type Wall to Pillar.
                matrix[pos.x, pos.y] = Util.ConvertTo<Terrain>((int)type + 1);
                break;
            case Terrain.MessageWall:
            case Terrain.MessagePillar:
            case Terrain.BloodMessageWall:
            case Terrain.BloodMessagePillar:
                break;
            default:
                throw new ArgumentException($"The terrain type:{matrix[pos.x, pos.y]} ({pos.x}, {pos.y}) is not suitable to set message.");
        }
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
                    // To avoid unreadable boards on the walls around stairs or boxes,
                    // forbid placing boards on the walls around dead end positions.
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
        int numOfBloodMessages = src.bloodMessages.Length;

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

                    // Reverse the other boards around the pit.
                    IDirection dir;
                    if (boardCandidates.TryGetValue(dest, out dir)) boardCandidates[dest] = BoardDirection(pos, dest, dir).Backward;
                });

                // Makes up for decrease of Pit Trap Attentions
                if (!isSet) numOfRandomMessages++;
            });
        }

        for (int i = 0; i < numOfBloodMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetBloodMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
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

    /// <summary>
    /// matrix[,] must be set before call this.
    /// </summary>
    private void CreateDirMap()
    {
        // Set direction to door and wall
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Skip pillar or gate
                if (IsGridPoint(x, y)) continue;

                dirMap[x, y] = GetNonGridPointDir(x, y);
            }
        }

        // Set pillar or gate
        for (int y = 0; y < height; y += 2)
        {
            for (int x = 0; x < width; x += 2)
            {
                dirMap[x, y] = GetGridPointDir(x, y);
            }
        }
    }

    public Dir CreateDir(int x, int y, Terrain terrain)
        => IsGridPoint(x, y) ? GetGridPointDir(x, y, terrain) : GetNonGridPointDir(x, y, terrain);

    private bool IsGridPoint(int x, int y) => x % 2 == 0 && y % 2 == 0;

    private Dir GetNonGridPointDir(int x, int y) => GetNonGridPointDir(x, y, matrix[x, y]);

    private Dir GetNonGridPointDir(int x, int y, Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.Door:
                return GetGateDir(x, y);

            case Terrain.LockedDoor:
                return GetValidDir(x, y, IsPath);

            case Terrain.MessageWall:
            case Terrain.BloodMessageWall:
            case Terrain.ExitDoor:
                return GetValidDir(x, y, IsEnterable);

            case Terrain.Box:
                return GetValidDir(x, y, terrain => IsEnterable(terrain) || terrain == Terrain.Door);
        }

        return GetWallDir(x, y);
    }

    private Dir GetGridPointDir(int x, int y) => GetGridPointDir(x, y, matrix[x, y]);

    private Dir GetGridPointDir(int x, int y, Terrain terrain)
    {
        if (terrain == Terrain.MessagePillar || terrain == Terrain.BloodMessagePillar || terrain == Terrain.Box)
        {
            return GetValidDir(x, y, IsEnterable);
        }

        Dir doorDir = GetDoorDir(x, y);

        // Leave it as is if strait wall or room ground
        if (doorDir == Dir.NONE)
        {
            Dir wallDir = GetWallDir(x, y);
            if (wallDir == Dir.NS || wallDir == Dir.EW || wallDir == Dir.NONE)
            {
                return wallDir;
            }
        }

        // Set gate as wall next to door
        // Set pillar as corner wall
        matrix[x, y] = Terrain.Pillar;

        return doorDir;
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
    public Dir GetDoorDir(int x, int y) => GetDir(x, y, Terrain.Door) | GetDir(x, y, Terrain.LockedDoor) | GetDir(x, y, Terrain.ExitDoor);
    private Dir GetWallDir(int x, int y) => GetDir(x, y, Terrain.Wall) | GetDir(x, y, Terrain.MessageWall) | GetDir(x, y, Terrain.BloodMessageWall);
    public Dir GetPillarDir(int x, int y) => GetDir(x, y, Terrain.Pillar) | GetDir(x, y, Terrain.MessagePillar) | GetDir(x, y, Terrain.BloodMessagePillar);

    public Dir GetValidDir(int x, int y) => GetValidDir(x, y, IsEnterable);

    private Dir GetGateDir(int x, int y)
    {
        Dir validDir = GetValidDir(x, y);
        Dir invDir = Direction.Convert(validDir).Backward.Enum;
        return Dir.NESW ^ validDir ^ invDir;
    }

    private Dir GetValidDir(int x, int y, Func<Terrain, bool> validChecker)
    {
        if (y > 0 && validChecker(matrix[x, y - 1])) return Dir.N;
        if (x > 0 && validChecker(matrix[x - 1, y])) return Dir.W;
        if (y < height - 2 && validChecker(matrix[x, y + 1])) return Dir.S;
        if (x < width - 2 && validChecker(matrix[x + 1, y])) return Dir.E;

        return Dir.NONE;
    }

    private bool IsEnterable(Terrain terrain) => terrain == Terrain.Ground || terrain == Terrain.Path;
    private bool IsPath(Terrain terrain) => terrain == Terrain.Path;
}
