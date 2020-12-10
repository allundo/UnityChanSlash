using System.Linq;
using System.Collections.Generic;
using Random = System.Random;

public class Dungeon
{
    private Queue<Pos> branchCandidate = new Queue<Pos>();

    public Direction InitDir { get; private set; }
    public Pos InitPos { get; private set; }

    public Terrain[,] Matrix { get; protected set; }
    public int Width { get; protected set; } = 50;
    public int Height { get; protected set; } = 50;

    public Dungeon(int width = 50, int height = 50)
    {
        this.Width = width;
        this.Height = height;
        this.Matrix = new Terrain[width, height];

        Clear();

        List<Pos> dividingRandomPos = GetDividingRandomPos(width, height, new Pos(0, 0), 3, 3);

        Random rnd = new Random();
        foreach (Pos pos in dividingRandomPos)
        {
            MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), pos);
        }

        MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), GetRandomPos(width, height, new Pos(0, 0), 3));
        MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), GetRandomPos(width, height, new Pos(0, 0), 3));
        MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), GetRandomPos(width, height, new Pos(0, 0), 3));
        MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), GetRandomPos(width, height, new Pos(0, 0), 3));
        MakeRoom(rnd.Next(4, 7), rnd.Next(4, 7), GetRandomPos(width, height, new Pos(0, 0), 3));

        new DiggingPathGenerator(this);
    }

    private void Clear(Terrain type = Terrain.None, Terrain outer = Terrain.Wall)
    {
        for (int i = 1; i < Width - 1; i++)
        {
            for (int j = 1; j < Height - 1; j++)
            {
                Matrix[i, j] = type;
            }
        }

        SetOuter(outer);
    }

    private void SetOuter(Terrain type = Terrain.Wall)
    {
        for (int i = 0; i < Width; i++)
        {
            Matrix[i, 0] = Matrix[i, Height - 1] = type;
        }

        for (int j = 1; j < Height - 1; j++)
        {
            Matrix[0, j] = Matrix[Width - 1, j] = type;
        }
    }

    private Pos GetRandomPos(int w, int h, Pos offset, int trim = 0)
    {
        Random rnd = new Random();
        return new Pos(offset.x + rnd.Next(0, w - trim), offset.y + rnd.Next(0, h - trim));
    }

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

    private void MakeRoom(int w, int h, Pos pos)
    {
        // Shrinks room size if exceeds region
        // Extends room size 1 block if touches outer wall
        if (pos.x + w > Width - 1) w = Width - pos.x;
        if (pos.y + h > Height - 1) h = Height - pos.y;

        // Extends room size 1 block if touches outer wall
        if (pos.x == 1)
        {
            pos.x = 0;
            w++;
        }
        if (pos.y == 1)
        {
            pos.y = 0;
            h++;
        }

        // Set corners as temporary wall
        SetCornerWall(pos.x, pos.y);
        SetCornerWall(pos.x, pos.y + h - 1);
        SetCornerWall(pos.x + w - 1, pos.y);
        SetCornerWall(pos.x + w - 1, pos.y + h - 1);

        // Set edge as branch candidate
        for (int i = pos.x + 1; i < pos.x + w - 1; i++)
        {
            SetBranch(i, pos.y);
            SetBranch(i, pos.y + h - 1);
        }
        for (int j = pos.y + 1; j < pos.y + h - 1; j++)
        {
            SetBranch(pos.x, j);
            SetBranch(pos.x + w - 1, j);
        }

        // Set surface as ground
        for (int i = pos.x + 1; i < pos.x + w - 1; i++)
        {
            for (int j = pos.y + 1; j < pos.y + h - 1; j++)
            {
                Matrix[i, j] = Terrain.Ground;
            }
        }
    }

    private void SetCornerWall(int x, int y)
    {
        switch (Matrix[x, y])
        {
            case Terrain.Ground:
            case Terrain.Wall:
                return;

            default:
                Matrix[x, y] = Terrain.TmpWall;
                return;
        }
    }

    private void SetBranch(int x, int y)
    {
        switch (Matrix[x, y])
        {
            case Terrain.Ground:
            case Terrain.Wall:
            case Terrain.TmpWall:
                return;

            default:
                Matrix[x, y] = Terrain.Branch;
                branchCandidate.Enqueue(new Pos(x, y));
                return;
        }
    }

    public bool IsOutWall(int x, int y)
    {
        return x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
    }

    public Terrain GetTerrain(Pos pos)
    {
        if (IsOutWall(pos.x, pos.y)) return Terrain.Wall;

        return Matrix[pos.x, pos.y];
    }

    private class DiggingPathGenerator
    {
        private Dungeon dungeon;

        private Direction north = new North();
        private Direction east = new East();
        private Direction south = new South();
        private Direction west = new West();

        public bool IsTunnel => Probability(60);
        public bool IsTurn => Probability(10);
        public bool IsBranch => Probability(40);

        public DiggingPathGenerator(Dungeon branchSetDungeon)
        {
            this.dungeon = branchSetDungeon;

            for (Pos branchPos = GetNewBranchPos(); !branchPos.IsNull; branchPos = GetNewBranchPos())
            {
                DrawPath(branchPos);
            }
        }

        public static bool Probability(int prob)
        {
            return new Random().Next(0, 100) < prob;
        }

        private void DrawPath(Pos branchPos)
        {
            if (GetTerrain(branchPos) != Terrain.Branch) return;

            Direction dir = GetDirection(branchPos);

            if (dir == null)
            {
                dir = GetDirection(branchPos, Terrain.Ground);

                if (dir == null)
                {
                    SetTerrain(branchPos, Terrain.Wall);
                    return;
                }

                SetTerrain(dir.GetLeft(branchPos), Terrain.Wall);
                SetTerrain(dir.GetRight(branchPos), Terrain.Wall);

                Pos backwardPos = dir.GetBackward(branchPos);
                switch (GetTerrain(backwardPos))
                {
                    case Terrain.Ground:
                        SetTerrain(branchPos, IsTunnel ? dir.Door : Terrain.Wall);
                        return;

                    case Terrain.Branch:
                        if (GetDirection(backwardPos) == null)
                        {
                            SetTerrain(branchPos, Terrain.Ground);
                            SetTerrain(backwardPos, IsTunnel ? dir.Door : Terrain.Wall);
                            return;
                        }

                        SetTerrain(branchPos, Terrain.Wall);
                        DrawPath(backwardPos);
                        return;

                    default:
                        SetTerrain(branchPos, Terrain.Wall);
                        return;
                }
            }

            SetTerrain(dir.GetLeft(branchPos), Terrain.Wall);
            SetTerrain(dir.GetRight(branchPos), Terrain.Wall);
            SetTerrain(branchPos, dir.Door);
            Digging(dir.GetForward(branchPos), dir);
        }

        private void Digging(Pos pos, Direction dir)
        {
            bool hasDoor = false;

            while (true)
            {
                SetTerrain(pos, Terrain.Ground);

                Pos forward = dir.GetForward(pos);

                if (GetTerrain(forward) == Terrain.None && IsTurn)
                {
                    SetTerrain(forward, Terrain.Wall);
                }

                switch (GetTerrain(forward))
                {
                    case Terrain.None:
                        SetWallOrBranch(dir.GetLeft(pos));
                        SetWallOrBranch(dir.GetRight(pos));
                        pos = forward;
                        continue;

                    case Terrain.Branch:
                        SetTerrain(forward, dir.Door);
                        hasDoor = true;
                        break;
                }

                Direction lr = SelectDirection(pos, dir, hasDoor);
                if (lr == null)
                {
                    return;
                }

                dir = lr;
                pos = lr.GetForward(pos);
            }
        }

        private void SetWallOrBranch(Pos pos)
        {
            Terrain terrain = GetTerrain(pos);
            if (terrain == Terrain.Wall || terrain == Terrain.TmpWall) return;

            if (IsBranch)
            {
                SetBranch(pos);
                return;
            }

            SetTerrain(pos, Terrain.Wall);
        }

        private void SetBranch(Pos pos)
        {
            if (dungeon.IsOutWall(pos.x, pos.y)) return;

            SetTerrain(pos, Terrain.Branch);
            dungeon.branchCandidate.Enqueue(pos);
        }

        private Direction SelectDirection(Pos pos, Direction dir, bool hasDoor)
        {
            Direction checkDir = Probability(50) ? dir.Left : dir.Right;

            Terrain checkTerrain = GetTerrain(checkDir.GetForward(pos));
            Terrain inverseTerrain = GetTerrain(checkDir.GetBackward(pos));

            if (checkTerrain == Terrain.None) return SetBackWalls(pos, checkDir);
            if (inverseTerrain == Terrain.None) return SetBackWalls(pos, checkDir.Backward);

            Direction endDir = checkTerrain != Terrain.Branch && inverseTerrain == Terrain.Branch ? checkDir.Backward : checkDir;
            Pos endPos = endDir.GetForward(pos);
            Terrain endTerrain = GetTerrain(endPos);

            if (!hasDoor)
            {
                if (endTerrain == Terrain.Branch)
                {
                    SetTerrain(endPos, endDir.Door);
                }
                else
                {
                    SetBranch(endPos);
                }
            }
            SetBackWalls(pos, endDir);
            return null;
        }

        private Direction SetBackWalls(Pos pos, Direction dir)
        {
            Pos backward = dir.GetBackward(pos);
            Pos leftback = dir.GetLeft(backward);
            Pos rightback = dir.GetRight(backward);

            SetWallOrBranch(backward);
            SetTerrain(leftback, Terrain.Wall);
            SetTerrain(rightback, Terrain.Wall);

            return dir;
        }

        private Direction GetDirection(Pos pos, Terrain terrain = Terrain.None)
        {
            foreach (Direction dir in new[] { north, east, south, west })
            {
                if (GetTerrain(dir.GetForward(pos)) == terrain) return dir;
            }
            return null;
        }

        private Pos GetNewBranchPos()
        {
            while (dungeon.branchCandidate.Count > 0)
            {
                Pos pos = dungeon.branchCandidate.Dequeue();
                if (GetTerrain(pos) == Terrain.Branch) return pos;
            }

            return new Pos(0, 0);
        }

        private Terrain GetTerrain(Pos pos)
        {
            return dungeon.GetTerrain(pos);
        }

        private void SetTerrain(Pos pos, Terrain terrain)
        {
            dungeon.Matrix[pos.x, pos.y] = terrain;
        }
    }
}

