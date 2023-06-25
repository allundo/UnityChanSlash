public class BaseMapData<T>
{
    public T[,] matrix { get; protected set; }

    public int width { get; protected set; }
    public int height { get; protected set; }

    public BaseMapData(T[,] matrix, int width, int height)
    {
        this.matrix = matrix;
        this.width = width;
        this.height = height;
    }
}

public class DirHandler<T> : BaseMapData<T>
{
    public DirHandler(T[,] matrix, int width, int height) : base(matrix, width, height) { }

    protected Dir GetDir(int x, int y, T type, T outRange)
    {
        Dir dir = Dir.NONE;

        T up = (y - 1 < 0) ? outRange : matrix[x, y - 1];
        T left = (x - 1 < 0) ? outRange : matrix[x - 1, y];
        T down = (y + 1 >= height) ? outRange : matrix[x, y + 1];
        T right = (x + 1 >= width) ? outRange : matrix[x + 1, y];

        if (up.Equals(type)) dir |= Dir.N;
        if (down.Equals(type)) dir |= Dir.S;
        if (left.Equals(type)) dir |= Dir.W;
        if (right.Equals(type)) dir |= Dir.E;

        return dir;
    }
}
