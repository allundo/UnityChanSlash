using System;
using System.Linq;
using System.Collections.Generic;

public class MapManager
{
    public int floor { get; private set; } = 0;
    public int width { get; private set; }
    public int height { get; private set; }

    public List<Pos> roomCenterPos { get; private set; } = new List<Pos>();
    public List<Pos> pitTrapPos { get; private set; } = new List<Pos>();
    public List<Pos> fixedMessagePos { get; private set; } = new List<Pos>();
    public List<Pos> bloodMessagePos { get; private set; } = new List<Pos>();

    public StairsPlacer stairsPlacer { get; protected set; }
    public Terrain[,] matrix { get; protected set; }
    public Dir[,] dirMap { get; protected set; }

    // Create maze
    public MapManager(int floor, int width = 49, int height = 49)
    {
        this.floor = floor;
        this.width = width;
        this.height = height;

        var maze = new MazeCreator(width, height);

        roomCenterPos = new List<Pos>(maze.roomCenterPos);

        stairsPlacer = new StairsPlacer(maze.matrix, floor, width, height);
        if (floor == 1) fixedMessagePos.Add(stairsPlacer.exitDoorMessage);

        matrix = stairsPlacer.matrix;
        dirMap = stairsPlacer.dirMap;

        SetPitAndMessageBoards(floor);
    }

    // Load from MapData
    public MapManager(int floor, DataStoreAgent.MapData mapData)
    {
        this.floor = floor;
        this.width = mapData.mapSize;
        this.height = mapData.mapMatrix.Length / width;
        this.roomCenterPos = mapData.roomCenterPos.ToList();

        this.stairsPlacer = new StairsPlacer(mapData);

        this.matrix = stairsPlacer.matrix;
        this.dirMap = stairsPlacer.dirMap;
    }

    // Custom map data with custom deadEndPos.
    public MapManager(CustomMapData data)
    {
        this.floor = data.floor;
        this.width = data.width;
        this.height = data.height;

        this.roomCenterPos = data.roomCenter;

        var fixedMessageBuffer = data.fixedMes;
        var bloodMessageBuffer = data.bloodMes;

        stairsPlacer = new StairsPlacer(data);

        matrix = stairsPlacer.matrix;
        dirMap = stairsPlacer.dirMap;

        FloorMessagesSource src = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);
        int numOfFixedMessage = src.fixedMessages.Length;
        int numOfBloodMessage = src.bloodMessages.Length;

        data.fixedMessagePos?.ForEach(kv =>
        {
            if (numOfFixedMessage-- > 0) SetFixedMessage(kv.Key, kv.Value);
        });

        for (int i = 0; i < data.fixedMes.Count && i < numOfFixedMessage; i++)
        {
            this.fixedMessagePos.Add(data.fixedMes[i]);
        }

        data.bloodMessagePos?.ForEach(kv =>
        {
            if (numOfBloodMessage-- > 0) SetBloodMessage(kv.Key, kv.Value);
        });

        int bloodCount = data.bloodMes.Count;
        int bloodOverCount = bloodCount - Math.Max(0, numOfBloodMessage);
        if (bloodOverCount > 0)
        {
            var bloodOver = data.bloodMes.GetRange(bloodCount - bloodOverCount, bloodOverCount);
            data.bloodMes.RemoveRange(bloodCount - bloodOverCount, bloodOverCount);

            // Convert surplus blood messages to random messages.
            bloodOver.ForEach(pos => stairsPlacer.dirMapData.SetBloodMessageToNormal(pos));
        }

        this.bloodMessagePos.AddRange(data.bloodMes);
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
                    if (!stairsPlacer.deadEndPos.ContainsKey(pos)) SetBoardCandidate(boardCandidates, x, y);
                }
                else
                {
                    pitCandidates.Add(pos);
                }
            }
        }

        // Forbid placing pit traps in front of items and stairs
        stairsPlacer.deadEndPos.ForEach(kv => { pitCandidates.Remove(kv.Value.GetForward(kv.Key)); });
        pitCandidates.Remove(stairsPlacer.StairsBottom);
        pitCandidates.Remove(stairsPlacer.StairsTop);

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
}
