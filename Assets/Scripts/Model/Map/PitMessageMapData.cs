using System;
using System.Linq;
using System.Collections.Generic;

public class PitMessageMapData : DirMapData
{
    public List<Pos> fixedMessagePos { get; private set; } = new List<Pos>();
    public List<Pos> bloodMessagePos { get; private set; } = new List<Pos>();
    public int numOfPits { get; private set; }

    // Create default data
    public PitMessageMapData(IDirMapData data) : base(data) { }

    // Newly created maze
    public PitMessageMapData(StairsMapData data, int floor) : base(data)
    {
        // Don't set pit in front of items(dead ends)
        List<Pos> pitIgnore = deadEndPos.Select(kv => kv.Value.GetForward(kv.Key)).ToList();
        pitIgnore.Add(data.StairsBottom);
        pitIgnore.Add(data.StairsTop);

        var boardCandidates = new Dictionary<Pos, IDirection>();
        var pitCandidates = GetPitAndMessageCandidates(boardCandidates, pitIgnore);

        FloorMessagesSource src = ResourceLoader.Instance.floorMessagesData.Param(floor - 1);

        int numOfFixedMessages = src.fixedMessages.Length;
        int numOfRandomMessages = src.randomMessages.Length;
        int numOfBloodMessages = src.bloodMessages.Length;

        numOfPits = new int[] { floor * floor * 4, width * height / 10, pitCandidates.Count }.Min();

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

        PlaceMessageBoards(boardCandidates, numOfFixedMessages, numOfRandomMessages, numOfBloodMessages);
    }

    // Custom map data with custom message board positions.
    public PitMessageMapData(DirMapHandler dirMapHandler, CustomMapData data) : base(dirMapHandler)
    {
        numOfPits = data.numOfPits;

        var fixedMessageBuffer = data.fixedMes;
        var bloodMessageBuffer = data.bloodMes;

        FloorMessagesSource src = ResourceLoader.Instance.floorMessagesData.Param(data.floor - 1);
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
            bloodOver.ForEach(pos => dirMapHandler.SetBloodMessageToNormal(pos));
        }

        this.bloodMessagePos.AddRange(data.bloodMes);
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
    /// <param name="pitIgnore">Exclude positions to place pits</param>
    /// <returns>Pit trap position candidates</returns>
    private List<Pos> GetPitAndMessageCandidates(Dictionary<Pos, IDirection> boardCandidates, List<Pos> pitIgnore)
    {
        var pitCandidates = new List<Pos>();

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
                    pitCandidates.Add(pos);
                }
            }
        }

        return pitCandidates.Filter(pitIgnore).Shuffle().ToList();
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

    private void PlaceMessageBoards(
        Dictionary<Pos, IDirection> boardCandidates,
        int numOfFixedMessages,
        int numOfRandomMessages,
        int numOfBloodMessages
    )
    {
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
