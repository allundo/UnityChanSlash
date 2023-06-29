using System;
using System.Linq;
using System.Collections.Generic;

public class PitMessageMapData : DirMapData
{
    public int floor { get; private set; }
    public List<Pos> fixedMessagePos { get; private set; } = new List<Pos>();
    public List<Pos> bloodMessagePos { get; private set; } = new List<Pos>();
    public Dictionary<Pos, int> randomMessagePos { get; private set; } = new Dictionary<Pos, int>();
    public Dictionary<Pos, int> secretMessagePos { get; private set; } = new Dictionary<Pos, int>();
    private Stack<int> randomIndices = null;
    private Stack<int> secretIndices = null;
    private FloorMessagesSource floorMessages;
    private SecretMessageData[] secretMessages;
    private SecretMessagesDataAsset secretMessagesData;

    public DataStoreAgent.PosList[] ExportRandomMessagePos()
        => ExportPosList(randomMessagePos, floorMessages.randomMessages.Length);

    public DataStoreAgent.PosList[] ExportSecretMessagePos()
        => ExportPosList(secretMessagePos, secretMessagesData.Length);

    private DataStoreAgent.PosList[] ExportPosList(Dictionary<Pos, int> messageMap, int numOfMessages)
    {
        // export[RANDOM_MESSAGE_ID] = List<PLACED_BOARD_POSITION>
        var export = Enumerable.Repeat(new List<Pos>(), numOfMessages).ToArray();
        messageMap.ForEach(kv => export[kv.Value].Add(kv.Key));
        return export.Select(posList => new DataStoreAgent.PosList(posList)).ToArray();
    }

    // Newly created maze
    public PitMessageMapData(StairsMapData data, int floor) : base(data)
    {
        this.floor = floor;

        var boardCandidates = new Dictionary<Pos, IDirection>();
        var pitCandidates = GetPitAndMessageCandidates(boardCandidates, data.deadEndPos);

        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);
        secretMessagesData = ResourceLoader.Instance.secretMessagesData;
        secretMessages = secretMessagesData.GetFloorMessages(floor, GameInfo.Instance.secretLevel);

        int numOfFixedMessages = floorMessages.fixedMessages.Length;
        int numOfRandomMessages = floorMessages.randomMessages.Length;

        var numOfPits = new int[] { floor * floor * 4, width * height / 10, pitCandidates.Count }.Min();

        if (floor == 1)
        {
            // numOfPits may be 4
            fixedMessagePos.Add(data.exitDoorMessage);
            numOfFixedMessages--;

            int count = PlacePitWithAttention(pitCandidates, boardCandidates, numOfPits);

            numOfFixedMessages -= count;

            // Makes up for decrease of Pit Trap Attentions
            numOfRandomMessages += (numOfPits - count);
        }
        else
        {
            pitCandidates.GetRange(0, numOfPits).ForEach(pos => SetPitTrap(pos));
        }

        PlaceMessageBoards(boardCandidates, numOfFixedMessages, numOfRandomMessages);
    }

    // Import map data
    public PitMessageMapData(IDirMapData data, int floor, DataStoreAgent.MapData import) : base(data)
    {
        this.floor = floor;
        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);
        secretMessagesData = ResourceLoader.Instance.secretMessagesData;

        fixedMessagePos = import.fixedMessagePos.ToList();
        bloodMessagePos = import.bloodMessagePos.ToList();

        for (int i = 0; i < import.randomMessagePos.Length; i++)
        {
            var posList = import.randomMessagePos[i];
            posList.pos.ForEach(pos => this.randomMessagePos[pos] = i);
        }

        for (int i = 0; i < import.secretMessagePos.Length; i++)
        {
            var posList = import.secretMessagePos[i];
            posList.pos.ForEach(pos => this.secretMessagePos[pos] = i);
        }
    }

    // Custom map data with custom message board positions.
    public PitMessageMapData(DirMapHandler data, CustomMapData custom) : base(data)
    {
        floor = custom.floor;
        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);
        secretMessagesData = ResourceLoader.Instance.secretMessagesData;
        secretMessages = secretMessagesData.GetFloorMessages(floor, GameInfo.Instance.secretLevel);

        var customMsg = new CustomMessagePos(data, floorMessages.fixedMessages.Length, Terrain.MessageWall, GetRandomIndex);
        customMsg.SetCustomDataPos(custom.fixedMessagePos, custom.randomMes);

        this.fixedMessagePos = customMsg.fixedMessagePos;
        this.randomMessagePos = customMsg.randomMessagePos;

        var customBloodMsg = new CustomMessagePos(data, floorMessages.bloodMessages.Length, Terrain.BloodMessageWall, GetSecretIndex);
        customBloodMsg.SetCustomDataPos(custom.bloodMessagePos, custom.secretMes);

        this.bloodMessagePos = customBloodMsg.fixedMessagePos;
        this.secretMessagePos = customBloodMsg.randomMessagePos;
    }

    public void ApplyMessages(ITile[,] matrix)
    {
        for (int i = 0; i < floorMessages.fixedMessages.Length && i < fixedMessagePos.Count; i++)
        {
            Pos pos = fixedMessagePos[i];
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = floorMessages.fixedMessages[i].Convert();
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        }

        for (int i = 0; i < floorMessages.bloodMessages.Length && i < bloodMessagePos.Count; i++)
        {
            Pos pos = bloodMessagePos[i];
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = floorMessages.bloodMessages[i].Convert();
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        }

        randomMessagePos.ForEach(kv =>
        {
            Pos pos = kv.Key;
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = floorMessages.randomMessages[kv.Value].Convert();
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        });

        secretMessagePos.ForEach(kv =>
        {
            Pos pos = kv.Key;
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = secretMessagesData.Convert(kv.Value);
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        });
    }

    private void SetPitTrap(Pos pos) => matrix[pos.x, pos.y] = Terrain.Pit;

    private void SetFixedMessage(Pos pos, IDirection boardDir)
    {
        fixedMessagePos.Add(pos);
        SetMessageBoard(pos, boardDir, Terrain.MessageWall);
    }

    private void SetBloodMessage(Pos pos, IDirection boardDir)
    {
        bloodMessagePos.Add(pos);
        SetMessageBoard(pos, boardDir, Terrain.BloodMessageWall);
    }

    private void SetRandomMessage(Pos pos, IDirection boardDir)
    {
        randomMessagePos[pos] = GetRandomIndex();
        SetMessageBoard(pos, boardDir, Terrain.MessageWall);
    }

    private void SetSecretMessage(Pos pos, IDirection boardDir)
    {
        secretMessagePos[pos] = GetSecretIndex();
        SetMessageBoard(pos, boardDir, Terrain.BloodMessageWall);
    }

    private int GetRandomIndex()
    {
        if (randomIndices == null || randomIndices.Count == 0)
        {
            randomIndices = Enumerable.Range(0, floorMessages.randomMessages.Length).Shuffle().ToStack();
        }

        return randomIndices.Pop();
    }

    private int GetSecretIndex()
    {
        if (secretIndices == null || secretIndices.Count == 0)
        {
            secretIndices = secretMessages.Select(data => data.messageID).ToStack();
        }

        return secretIndices.Pop();
    }

    private void SetMessageBoard(Pos pos, IDirection boardDir, Terrain type = Terrain.MessageWall)
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

    /// <summary>
    /// Retrieve available pit and message placing positions.
    /// </summary>
    /// <param name="boardCandidates">Empty dictionary to take message board position candidates</param>
    /// <returns>Pit trap position candidates</returns>
    private List<Pos> GetPitAndMessageCandidates(Dictionary<Pos, IDirection> boardCandidates, Dictionary<Pos, IDirection> deadEndPos)
    {
        var pitCandidates = new List<Pos>();
        List<Pos> pitIgnore = deadEndPos.Select(kv => kv.Value.GetForward(kv.Key)).ToList();

        for (int y = 1; y < height; y++)
        {
            for (int x = 1; x < width; x++)
            {
                // Allow placing pits on ground or path
                if (matrix[x, y] != Terrain.Ground && matrix[x, y] != Terrain.Path) continue;

                Pos pos = new Pos(x, y);

                if ((x + y) % 2 == 0)
                {
                    // To avoid unreadable boards on the walls around stairs or boxes,
                    // forbid placing boards on the walls around dead end positions.
                    if (!deadEndPos.ContainsKey(pos)) SetBoardCandidate(boardCandidates, pos);
                }
                else
                {
                    // Forbid placing pit trap in front of items and stairs.
                    if (!pitIgnore.Contains(pos)) pitCandidates.Add(pos);
                }
            }
        }

        return pitCandidates.Shuffle().ToList();
    }

    /// <summary>
    /// Place pit with message board attention for B1F
    /// </summary>
    /// <param name="pitTrapPos">Position list to place pits</param>
    /// <param name="boardCandidates">Available message board placing position with board facing direction</param>
    /// <param name="numOfPits">Max number of placing pits</param>
    /// <returns></returns>
    private int PlacePitWithAttention(List<Pos> pitTrapPos, Dictionary<Pos, IDirection> boardCandidates, int numOfPits)
    {
        int placeCount = 0;

        foreach (Pos pos in pitTrapPos)
        {
            // Try setting pit attention message to north-west, north-east, east-south, south-west wall of each pit position.
            var targets = new Pos[] { pos.DecX().DecY(), pos.IncX().DecY(), pos.IncX().IncY(), pos.DecX().IncY() }
                .Intersect(boardCandidates.Keys)
                .ToList();

            if (targets.Count > 0)
            {
                var available = targets.Shuffle().ToStack();

                Pos boardPos = available.Pop();

                SetFixedMessage(boardPos, PitFacedDir(pos, boardPos, boardCandidates[boardPos]));
                SetPitTrap(pos);

                boardCandidates.Remove(boardPos);

                available.ForEach(other => boardCandidates[other] = PitFacedDir(pos, other, boardCandidates[other]));

                if (++placeCount == numOfPits) break;
            }
        }

        return placeCount;
    }

    private void PlaceMessageBoards(Dictionary<Pos, IDirection> boardCandidates, int numOfFixedMessages, int numOfRandomMessages)
    {
        for (int i = 0; i < floorMessages.bloodMessages.Length && boardCandidates.Count > 0; i++)
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
            SetRandomMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
        }

        for (int i = 0; i < secretMessages.Length && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetSecretMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
        }
    }

    private IDirection PitFacedDir(Pos pitPos, Pos boardPos, IDirection boardDir)
    {
        Pos vecToReadPos = boardDir.GetForward(boardPos) - pitPos;
        switch (Math.Abs(vecToReadPos.x) + Math.Abs(vecToReadPos.y))
        {
            case 1: return boardDir;
            case 3: return boardDir.Backward;
            default: throw new ArgumentException("Invalid pair of pit and board position.");
        }
    }

    private bool SetBoardCandidate(Dictionary<Pos, IDirection> candidates, Pos pos)
    {
        var dirMap = new Dictionary<Pos, IDirection>()
        {
            { pos.IncY(), Direction.north   },
            { pos.DecX(), Direction.east    },
            { pos.DecY(), Direction.south   },
            { pos.IncX(), Direction.west    },
        }.Shuffle();

        Pos targetPos = dirMap.Keys.FirstOrDefault(target => !candidates.Keys.Contains(target) && matrix[target.x, target.y] == Terrain.Wall);

        if (!targetPos.IsNull)
        {
            candidates[targetPos] = dirMap[targetPos];
            return true;
        }

        return false;
    }
}
