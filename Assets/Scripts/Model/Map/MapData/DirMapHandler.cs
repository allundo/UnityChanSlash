public class DirMapHandler : DirMapData
{
    public DirMapHandler(IDirMapData data) : base(data) { }

    private int[] ConvertToArray<T>(T[,] data) where T : System.Enum
    {
        var ret = new int[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ret[x + y * width] = System.Convert.ToInt32(data[x, y]);
            }
        }
        return ret;
    }

    public Terrain[,] CloneMatrix() => matrix.Clone() as Terrain[,];
    public Dir[,] CloneDirMap() => dirMap.Clone() as Dir[,];

    public int[] ConvertMapData() => ConvertToArray(matrix);
    public int[] ConvertDirData() => ConvertToArray(dirMap);

    public void SetBloodMessageToNormal(Pos pos)
    {
        switch (matrix[pos.x, pos.y])
        {
            case Terrain.BloodMessageWall:
                matrix[pos.x, pos.y] = Terrain.MessageWall;
                break;
            case Terrain.BloodMessagePillar:
                matrix[pos.x, pos.y] = Terrain.MessagePillar;
                break;
        }
    }

    public bool Unlock(Pos pos)
    {
        if (matrix[pos.x, pos.y] == Terrain.LockedDoor)
        {
            matrix[pos.x, pos.y] = Terrain.Door;
            return true;
        }

        return false;
    }

    public Dir GetPillarDir(int x, int y) => GetDir(x, y, Terrain.Pillar) | GetDir(x, y, Terrain.MessagePillar) | GetDir(x, y, Terrain.BloodMessagePillar);
    private Dir GetDir(int x, int y, Terrain type) => GetDir(x, y, type, Terrain.Ground);
}