using System.Linq;
using System.Collections.Generic;

public class CustomMap
{
    private Dictionary<Pos, IDirection> deadEndPos = new Dictionary<Pos, IDirection>();
    private int width;
    private int height;
    private int[] matrix;
    private int Matrix(int x, int y) => matrix[y * width + x];

    private bool isCustomDeadEnds = false;

    public CustomMap(int width, int[] matrix, Dictionary<Pos, IDirection> deadEndPos = null)
    {
        this.width = width;
        this.height = matrix.Length / width;
        this.matrix = matrix;

        this.isCustomDeadEnds = deadEndPos != null;

        if (this.isCustomDeadEnds)
        {
            this.deadEndPos = deadEndPos;
        }
        else
        {
            GetPaths().ForEach(pos => SetDeadEnd(pos.x, pos.y));
        }
    }

    private (int x, int y)[] GetPaths()
    {
        var list = new List<(int, int)>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Matrix(x, y) == 0) list.Add((x, y));
            }
        }

        return list.ToArray();
    }

    private void SetDeadEnd(int x, int y)
    {
        var list = new List<IDirection>();

        if (Matrix(x, y - 1) != 2) list.Add(Direction.north);
        if (Matrix(x, y + 1) != 2) list.Add(Direction.south);
        if (Matrix(x - 1, y) != 2) list.Add(Direction.west);
        if (Matrix(x + 1, y) != 2) list.Add(Direction.east);

        if (list.Count == 1) deadEndPos[new Pos(x, y)] = list[0];
    }

    public WorldMap CreateMap(int floor)
    {
        var mapManager = new MapManager(matrix, width, deadEndPos).SetDownStairs();

        if (isCustomDeadEnds)
        {
            mapManager.SetUpStairs(deadEndPos.Last().Key);
        }
        else
        {
            mapManager.SetUpStairs();
        }

        return new WorldMap(mapManager, floor);
    }
}
