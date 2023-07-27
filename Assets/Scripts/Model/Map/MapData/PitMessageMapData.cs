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
    private MessageData[] fixedMessages;
    private BloodMessageData[] bloodMessages;

    private MessageData[][] randomMessages;
    private MessageData RandomMessage(int index) => GetMessage(randomMessages, index);

    private SecretMessageData[][] secretMessages;
    private SecretMessageData SecretMessage(int index) => GetMessage(secretMessages, index);
    private int numOfSecretMessages;

    private T GetMessage<T>(T[][] messages, int index) where T : MessageData
        => messages[index / FloorMessagesData.MAX_ELEMENTS][index % FloorMessagesData.MAX_ELEMENTS];

    public DataStoreAgent.PosList[] ExportRandomMessagePos() => ExportPosList(randomMessagePos);
    public DataStoreAgent.PosList[] ExportSecretMessagePos() => ExportPosList(secretMessagePos);

    private DataStoreAgent.PosList[] ExportPosList(Dictionary<Pos, int> messageMap)
    {
        int numOfMessages = FloorMessagesData.MAX_ELEMENTS * randomMessages.Length;

        // export[(FLOOR-1) * MAX_ELEMENTS + FLOOR_MESSAGE_ID] = List<PLACED_BOARD_POSITION>
        var export = new List<Pos>[numOfMessages].Select(_ => new List<Pos>()).ToArray();
        messageMap.ForEach(kv => export[kv.Value].Add(kv.Key));
        return export.Select(posList => new DataStoreAgent.PosList(posList)).ToArray();
    }

    // Newly created maze
    public PitMessageMapData(StairsMapData data, int floor) : base(data)
    {
        this.floor = floor;

        var boardCandidates = new Dictionary<Pos, IDirection>();
        var pitCandidates = GetPitAndMessageCandidates(boardCandidates, data.deadEndPos);

        LoadMessages();

        int numOfFixedMessages = fixedMessages.Length;
        int numOfRandomMessages = randomMessages.Length;

        var numOfPits = new int[] { floor * floor * 4, width * height / 10, pitCandidates.Count }.Min();

        int numOfFloorMessages;
        if (floor == 1)
        {
            // Exclude exit door message and pit attention message.
            numOfFixedMessages -= 2;

            // Assign exit door message to fixed message list.
            var fixedMsgList = new List<MessageData>();
            fixedMsgList.Add(fixedMessages[0]);

            fixedMessagePos.Add(data.exitDoorMessage);

            // numOfPits may be 4
            int count = PlacePitWithAttention(pitCandidates, boardCandidates, numOfPits);

            // Assign pit attentions to fixed message list.
            for (int i = 0; i < count; ++i)
            {
                fixedMsgList.Add(fixedMessages[1]);
            }

            // Assign remaining fixed messages to fixed message list.
            for (int i = 2; i < fixedMessages.Length; ++i)
            {
                fixedMsgList.Add(fixedMessages[i]);
            }

            // Apply fixed message list including exit door message and pit attentions to fixed messages array.
            fixedMessages = fixedMsgList.ToArray();

            // Makes up for decrease of Pit Trap Attentions.
            numOfFloorMessages = numOfFixedMessages + randomMessages.Length + (numOfPits - count);
        }
        else
        {
            numOfFloorMessages = width / 2;
            pitCandidates.GetRange(0, numOfPits).ForEach(pos => SetPitTrap(pos));
        }

        PlaceMessageBoards(boardCandidates, numOfFixedMessages, numOfSecretMessages, numOfFloorMessages);
    }

    // Import map data
    public PitMessageMapData(IDirMapData data, int floor, DataStoreAgent.MapData import) : base(data)
    {
        this.floor = floor;
        LoadMessages();

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

        if (floor == 1) throw new ArgumentOutOfRangeException("CustomMap is not supported for floor1");

        LoadMessages();

        int numOfFloorMessages = data.width / 2;

        var remainingSecret = SetBloodDataPos(custom.bloodMessagePos, custom.secretMes, numOfSecretMessages);

        remainingSecret.AddRange(custom.randomMes);
        int numOfRandomMessages = numOfFloorMessages - fixedMessages.Length - bloodMessages.Length - numOfSecretMessages;
        var remainingRandom = SetMessageDataPos(custom.fixedMessagePos, remainingSecret, numOfRandomMessages);

        RemoveMessageFromWall(remainingRandom);
    }

    private List<Pos> SetMessageDataPos(Dictionary<Pos, IDirection> fixedCustomPos, List<Pos> randomCustomPos, int numOfRandom)
        => SetCustomDataPos(fixedCustomPos, randomCustomPos, fixedMessages.Length, numOfRandom, SetFixedMessage, SetRandomMessage);

    private List<Pos> SetBloodDataPos(Dictionary<Pos, IDirection> bloodCustomPos, List<Pos> secretCustomPos, int numOfSecret)
        => SetCustomDataPos(bloodCustomPos, secretCustomPos, bloodMessages.Length, numOfSecret, SetBloodMessage, SetSecretMessage);

    private List<Pos> SetCustomDataPos(
        Dictionary<Pos, IDirection> fixedCustomPos, List<Pos> randomCustomPos,
        int numOfFixed, int numOfRandom,
        Action<Pos, IDirection> fixedSetter, Action<Pos, IDirection> randomSetter
    )
    {
        var fixedPos = fixedCustomPos.Keys.ToList();

        int count;
        for (count = 0; count < numOfFixed && count < fixedPos.Count; ++count)
        {
            Pos pos = fixedPos[count];
            fixedSetter(pos, fixedCustomPos[pos]);
        }

        var randomPos = randomCustomPos.ToList();

        // Add fixed message pos to random message pos if all of fixed message are placed. 
        for (int i = 0; i < fixedPos.Count - count; ++i)
        {
            Pos pos = fixedPos[count + i];
            dirMap[pos.x, pos.y] = rawMapData.GetValidDir(pos.x, pos.y);
            randomPos.Add(pos);
        }

        var randomPosStack = randomPos.Shuffle().ToStack();

        // Use random message pos if custom fixed pos is not enough for fixed messages.
        for (int i = 0; i < numOfFixed - count && randomPosStack.Count > 0; ++i)
        {
            Pos pos = randomPosStack.Pop();
            fixedSetter(pos, null);
        }

        for (int i = 0; i < numOfRandom && randomPosStack.Count > 0; ++i)
        {
            randomSetter(randomPosStack.Pop(), null);
        }

        return randomPosStack.ToList();
    }

    private void RemoveMessageFromWall(List<Pos> messagePos)
    {
        messagePos.ForEach(pos =>
        {
            switch (matrix[pos.x, pos.y])
            {
                case Terrain.MessageWall:
                case Terrain.BloodMessageWall:
                    matrix[pos.x, pos.y] = Terrain.Wall;
                    dirMap[pos.x, pos.y] = rawMapData.GetWallDir(pos.x, pos.y);
                    break;
                case Terrain.MessagePillar:
                case Terrain.BloodMessagePillar:
                    matrix[pos.x, pos.y] = Terrain.Pillar;
                    dirMap[pos.x, pos.y] = rawMapData.GetDoorDir(pos.x, pos.y);
                    break;
            }
        });
    }

    private void LoadMessages()
    {
        var data = ResourceLoader.Instance.floorMessagesData;
        randomMessages = data.GetRandomMessages();
        secretMessages = data.GetSecretMessages(floor, GameInfo.Instance.secretLevel);

        floorMessages = data.Param(floor - 1);
        fixedMessages = floorMessages.fixedMessages.Select(mes => mes.Convert()).ToArray();
        bloodMessages = floorMessages.bloodMessages.Select(src => new BloodMessageData(src)).ToArray();
        numOfSecretMessages = Math.Min(floorMessages.secretMessages.Length, GameInfo.Instance.secretLevel + 1);
    }

    public void ApplyMessages(ITile[,] matrix)
    {
        for (int i = 0; i < fixedMessages.Length && i < fixedMessagePos.Count; i++)
        {
            Pos pos = fixedMessagePos[i];
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = fixedMessages[i];
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        }

        for (int i = 0; i < bloodMessages.Length && i < bloodMessagePos.Count; i++)
        {
            Pos pos = bloodMessagePos[i];
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = bloodMessages[i];
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        }

        randomMessagePos.ForEach(kv =>
        {
            Pos pos = kv.Key;
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = RandomMessage(kv.Value);
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        });

        secretMessagePos.ForEach(kv =>
        {
            Pos pos = kv.Key;
            var messageWall = matrix[pos.x, pos.y] as MessageWall;
            messageWall.data = SecretMessage(kv.Value);
            messageWall.boardDir = Direction.Convert(dirMap[pos.x, pos.y]);
        });
    }

    private void SetPitTrap(Pos pos) => matrix[pos.x, pos.y] = Terrain.Pit;

    private void SetFixedMessage(Pos pos, IDirection boardDir = null)
    {
        fixedMessagePos.Add(pos);
        SetMessageBoard(pos, Terrain.MessageWall, boardDir);
    }

    private void SetBloodMessage(Pos pos, IDirection boardDir = null)
    {
        bloodMessagePos.Add(pos);
        SetMessageBoard(pos, Terrain.BloodMessageWall, boardDir);
    }

    private void SetRandomMessage(Pos pos, IDirection boardDir = null)
    {
        randomMessagePos[pos] = GetRandomIndex();
        SetMessageBoard(pos, Terrain.MessageWall, boardDir);
    }

    private void SetSecretMessage(Pos pos, IDirection boardDir = null)
    {
        secretMessagePos[pos] = GetSecretIndex();
        SetMessageBoard(pos, Terrain.BloodMessageWall, boardDir);
    }

    private int GetRandomIndex() => GetIndex(randomIndices, randomMessages);
    private int GetSecretIndex() => GetIndex(secretIndices, secretMessages);

    private int GetIndex<T>(Stack<int> indices, T[][] messages) where T : MessageData
    {
        if (indices == null)
        {
            indices = GetFloorIndices(floor, messages).Shuffle().ToStack();
        }

        // Fall down to lower depth floor messages if all of floor messages are used.
        if (indices.Count == 0)
        {
            var list = new List<int>();
            for (int i = 1; i < floor; ++i)
            {
                list.AddRange(GetFloorIndices(i, messages));
            }

            indices = (list.Count > 0 ? list : GetFloorIndices(floor, messages)).Shuffle().ToStack();
        }

        return indices.Pop();
    }

    private IEnumerable<int> GetFloorIndices<T>(int floor, T[][] messages) where T : MessageData
    {
        int offset = (floor - 1) * FloorMessagesData.MAX_ELEMENTS;
        return Enumerable.Range(offset, messages[floor - 1].Length);
    }

    private void SetMessageBoard(Pos pos, Terrain type = Terrain.MessageWall, IDirection boardDir = null)
    {
        if (boardDir != null) dirMap[pos.x, pos.y] = boardDir.Enum;

        switch (matrix[pos.x, pos.y])
        {
            case Terrain.Wall:
            case Terrain.MessageWall:
            case Terrain.BloodMessageWall:
                matrix[pos.x, pos.y] = type;
                break;
            case Terrain.Pillar:
            case Terrain.MessagePillar:
            case Terrain.BloodMessagePillar:
                // Convert terrain type Wall to Pillar.
                matrix[pos.x, pos.y] = Util.ConvertTo<Terrain>((int)type + 1);
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
    /// <returns>Number of placed pits</returns>
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

    private void PlaceMessageBoards(Dictionary<Pos, IDirection> boardCandidates, int numOfFixedMessages, int numOfSecretMessages, int numOfFloorMessages)
    {
        for (int i = 0; i < floorMessages.bloodMessages.Length && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetBloodMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
            numOfFloorMessages--;
        }

        for (int i = 0; i < numOfFixedMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetFixedMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
            numOfFloorMessages--;
        }

        for (int i = 0; i < numOfSecretMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetSecretMessage(pos, boardCandidates[pos]);
            boardCandidates.Remove(pos);
            numOfFloorMessages--;
        }

        for (int i = 0; i < numOfFloorMessages && boardCandidates.Count > 0; i++)
        {
            Pos pos = boardCandidates.GetRandomKey();
            SetRandomMessage(pos, boardCandidates[pos]);
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
