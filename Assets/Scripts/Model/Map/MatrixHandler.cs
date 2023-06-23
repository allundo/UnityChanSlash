using System.Collections.Generic;

public class MatrixHandler
{
    private int width;
    private int height;
    private int[] matrix;
    private int Matrix(int x, int y) => matrix[y * width + x];
    public MatrixHandler(int[] matrix, int width)
    {
        this.width = width;
        this.height = matrix.Length / width;
        this.matrix = matrix;
    }

    private Pos[] GetPaths()
    {
        var list = new List<Pos>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Matrix(x, y) == 0) list.Add(new Pos(x, y));
            }
        }

        return list.ToArray();
    }

    public Dictionary<Pos, IDirection> SearchDeadEnds()
    {
        var deadEndPos = new Dictionary<Pos, IDirection>();

        GetPaths().ForEach(pos =>
        {
            var list = new List<IDirection>();

            if (Matrix(pos.x, pos.y - 1) != 2) list.Add(Direction.north);
            if (Matrix(pos.x, pos.y + 1) != 2) list.Add(Direction.south);
            if (Matrix(pos.x - 1, pos.y) != 2) list.Add(Direction.west);
            if (Matrix(pos.x + 1, pos.y) != 2) list.Add(Direction.east);

            if (list.Count == 1) deadEndPos.Add(pos, list[0]);
        });

        return deadEndPos;
    }
}
