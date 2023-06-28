using System;

public class TileMapHandler : TileMapData
{
    protected TileMapHandler(ITile[,] matrix, int floor, int width, int height) : base(matrix, floor, width, height)
    { }
    public void ForEachTiles(Action<ITile> action)
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                action(matrix[i, j]);
            }
        }
    }
    public void ForEachTiles(Action<ITile, Pos> action)
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                action(matrix[i, j], new Pos(i, j));
            }
        }
    }
}
