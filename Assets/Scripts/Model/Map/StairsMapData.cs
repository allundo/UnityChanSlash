using System;
using System.Linq;
using System.Collections.Generic;

public interface IStairsData
{
    Pos upStairs { get; }
    Pos downStairs { get; }
    Pos exitDoor { get; }
}

public class StairsMapData : DirMapData, IStairsData
{
    public Pos upStairs { get; private set; }
    public Pos downStairs { get; private set; }
    public Pos exitDoor { get; private set; }
    public Pos exitDoorMessage { get; private set; }

    public Pos StairsBottom => upStairs.IsNull ? GetStartPos(exitDoor) : GetStartPos(upStairs);
    public Pos StairsTop => GetStartPos(downStairs);

    public IDirection UpStairsDir => upStairs.IsNull ? GetValidDir(GetStartPos(exitDoor)) : GetDir(upStairs);
    public IDirection DownStairsDir => GetDir(downStairs);

    public IDirection GetDir(Pos pos) => Direction.Convert(dirMap[pos.x, pos.y]);
    public Pos GetStartPos(Pos stairsPos) => stairsPos.IsNull ? new Pos() : GetDir(stairsPos).GetForward(stairsPos);
    private IDirection GetValidDir(Pos pos) => Direction.Convert(rawMapData.GetValidDir(pos.x, pos.y));


    // Imported from sava data or created from custom map data
    public StairsMapData(IDirMapData mapData, IStairsData stairsData) : base(mapData)
    {
        downStairs = stairsData.downStairs;
        upStairs = stairsData.upStairs;
        exitDoor = stairsData.exitDoor;

        // Don't set door in front of stairs
        SetGround(StairsBottom);
        SetGround(StairsTop);
    }

    // New created data
    public StairsMapData(IDirMapData data, int floor) : base(data)
    {
        downStairs = upStairs = exitDoor = new Pos();

        if (floor < GameInfo.Instance.LastFloor)
        {
            downStairs = deadEndPos.First().Key;
            SetStairs(downStairs, deadEndPos[downStairs], Terrain.DownStairs);
            deadEndPos.Remove(downStairs);
        }

        if (floor == 1)
        {
            Pos startPos = GetStartPos(downStairs, deadEndPos);
            exitDoor = SetStartPos(startPos, deadEndPos[startPos]);
            deadEndPos.Remove(startPos);
        }
        else
        {
            upStairs = GetUpStairsPos(downStairs, deadEndPos);
            SetStairs(upStairs, deadEndPos[upStairs], Terrain.UpStairs);
            deadEndPos.Remove(upStairs);
        }
    }

    private Pos GetUpStairsPos(Pos downStairs, Dictionary<Pos, IDirection> deadEndPos)
    {
        if (downStairs.IsNull) return deadEndPos.Last().Key;

        int minDistance = Math.Min(width, height);
        Pos pos = deadEndPos.Keys.FirstOrDefault(pos => pos.Distance(downStairs) > minDistance);
        if (!pos.IsNull) return pos;

        return deadEndPos.Keys.OrderBy(pos => pos.Distance(downStairs)).Last();
    }

    private Pos SetStartPos(Pos pos, IDirection dir)
    {
        Pos doorPos;

        if (IsEdgeWall(doorPos = dir.GetLeft(pos)))
        {
            SetExitDoor(doorPos, dir.Right);
            SetMessageWall(dir.GetBackward(pos), dir);
        }
        else if (IsEdgeWall(doorPos = dir.GetRight(pos)))
        {
            SetExitDoor(doorPos, dir.Left);
            SetMessageWall(dir.GetBackward(pos), dir);
        }
        else if (IsEdgeWall(doorPos = dir.GetBackward(pos)))
        {
            SetExitDoor(doorPos, dir);
            SetMessageWall(dir.GetLeft(pos), dir.Right);
        }
        else
        {
#if UNITY_EDITOR
            rawMapData.DebugMatrix();
#endif
            throw new Exception("Position: (" + pos.x + ", " + pos.y + ") isn't suitable to start.");
        }

        // Don't set door in front of player start position
        SetGround(dir.GetForward(pos));

        return doorPos;
    }

    private Pos GetStartPos(Pos downStairs, Dictionary<Pos, IDirection> deadEndPos)
    {
        int minDistance = Math.Min(width, height);
        var nextToEdge = deadEndPos.Keys.Where(pos => IsNextToEdgeWall(pos));
        Pos startPos = nextToEdge.FirstOrDefault(pos => pos.Distance(downStairs) > minDistance);

        if (!startPos.IsNull) return startPos;

        var cornerPosList = new Pos[] { new Pos(1, 1), new Pos(width - 2, 1), new Pos(1, height - 2), new Pos(width - 2, height - 2) }
            .OrderByDescending(pos => pos.Distance(downStairs));

        foreach (Pos pos in cornerPosList)
        {
            var dir = GetValidDir(pos);
            if (dir != null)
            {
                deadEndPos[pos] = dir;
                return pos;
            }
        }

        Pos farthest = nextToEdge.OrderBy(pos => pos.Distance(downStairs)).Last();
        if (!farthest.IsNull) return farthest;

        throw new InvalidOperationException("Failed to create ExitDoor.");
    }

    private bool IsEdgeWall(Pos pos) => pos.x == 0 || pos.x == width - 1 || pos.y == 0 || pos.y == height - 1;
    private bool IsNextToEdgeWall(Pos pos) => pos.x == 1 || pos.x == width - 2 || pos.y == 1 || pos.y == height - 2;

    private void SetStairs(Pos pos, IDirection dir, Terrain stairs)
    {
        matrix[pos.x, pos.y] = stairs;
        dirMap[pos.x, pos.y] = dir.Enum;

        // Don't set door in front of stairs
        SetGround(dir.GetForward(pos));
    }

    private void SetGround(Pos pos)
    {
        if (!pos.IsNull) matrix[pos.x, pos.y] = Terrain.Ground;
    }

    private void SetMessageWall(Pos pos, IDirection messageDir)
    {
        matrix[pos.x, pos.y] = Terrain.MessageWall;
        dirMap[pos.x, pos.y] = messageDir.Enum;
        exitDoorMessage = pos;
    }

    private void SetExitDoor(Pos pos, IDirection doorDir)
    {
        // Set exit door
        matrix[pos.x, pos.y] = Terrain.ExitDoor;
        dirMap[pos.x, pos.y] = doorDir.Enum;

        // Set gate to both side of the exit door
        Pos leftPos = doorDir.GetLeft(pos);
        Pos rightPos = doorDir.GetRight(pos);

        matrix[leftPos.x, leftPos.y] = matrix[rightPos.x, rightPos.y] = Terrain.Pillar;
        dirMap[leftPos.x, leftPos.y] |= doorDir.Right.Enum;
        dirMap[rightPos.x, rightPos.y] |= doorDir.Left.Enum;
    }
}
