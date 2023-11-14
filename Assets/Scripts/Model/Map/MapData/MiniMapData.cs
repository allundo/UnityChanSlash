using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MiniMapData : TileMapData
{
    public Texture2D texMap { get; protected set; }
    public bool[,] discovered { get; protected set; }

    private List<Pos> currentViewOpen = new List<Pos>();
    public void ClearCurrentViewOpen() => currentViewOpen.Clear();
    public bool IsCurrentViewOpen(Vector3 worldPos) => IsCurrentViewOpen(MapPos(worldPos));
    public bool IsCurrentViewOpen(Pos pos) => currentViewOpen.Contains(pos);

    /// <summary>
    /// The tile can be seen through
    /// </summary>
    public bool IsTileViewOpen(int x, int y) => !IsOutOfRange(x, y) && matrix[x, y].IsViewOpen;

    public static MiniMapData Convert(Terrain[,] matrix, int floor, int width, int height)
    {
        var tileMatrix = new ITile[width, height];

        var pixels = new Color[width * height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tileMatrix[i, j] = TileMapData.ConvertToTile(matrix[i, j], floor);
                pixels[i + width * j] = MiniMapData.ConvertToColor(matrix[i, j]);
            }
        }

        return new MiniMapData(pixels, tileMatrix, floor, width, height);
    }

    protected MiniMapData(Color[] pixels, ITile[,] matrix, int floor, int width, int height) : base(matrix, floor, width, height)
    {
        texMap = new Texture2D(width, height, TextureFormat.RGB24, false);
        texMap.SetPixels(pixels);
        texMap.Apply();

        // The elements are initialized by false automatically
        discovered = new bool[width, height];
    }

    public void SetDiscovered(Pos pos)
    {
        currentViewOpen.Add(pos);

        for (int i = pos.x - 1; i <= pos.x + 1; i++)
        {
            for (int j = pos.y - 1; j <= pos.y + 1; j++)
            {
                discovered[i, j] = true;
            }
        }
    }

    public int SumUpDiscovered()
    {
        int discoveredCount = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (discovered[x, y]) discoveredCount++;
            }
        }
        return discoveredCount;
    }

    public Texture2D GetMiniMap(int mapSize = 15)
    {
        Pos pos = MiniMapCenterPos(mapSize);

        int x = pos.x - mapSize / 2;
        int y = pos.y - mapSize / 2;

        Color[] pixels = texMap.GetPixels(x, y, mapSize, mapSize);

        var texture = new Texture2D(mapSize, mapSize);
        texture.SetPixels(MiniMapPixels(pixels, x, y, mapSize, mapSize));
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;
    }

    private Pos MiniMapCenterPos(int mapSize = 15)
    {
        Pos pos = PlayerInfo.Instance.Pos;
        int half = mapSize / 2;

        return new Pos(
            Mathf.Clamp(pos.x, half, width - half - 1),
            Mathf.Clamp(pos.y, half, height - half - 1)
        );
    }

    private Color[] MiniMapPixels(Color[] texPixels, int x, int y, int blockWidth, int blockHeight)
    {
        Color[] pixels = Enumerable.Repeat(Color.clear, texPixels.Length).ToArray();

        for (int j = 0; j < blockHeight; j++)
        {
            for (int i = 0; i < blockWidth; i++)
            {
                if (!discovered[x + i, y + j]) continue;

                int inverseJ = blockHeight - j - 1;
                Door door = matrix[x + i, y + j] as Door;

                Color color;
                if (door != null && !(door is ExitDoor))
                {
                    float red =
                        door.IsOpen ? 0.75f
                        : door.IsBroken ? 0.25f
                        : 1f;

                    color = new Color(red, 0f, 0f, 1f);
                }
                else
                {
                    color = texPixels[i + j * blockWidth];
                }

                pixels[i + inverseJ * blockWidth] = color;
            }
        }

        return pixels;
    }

    /// <summary>
    /// Convert mini map center TILE POSITION to world position by Vector3. <br />
    /// The mini map size must be 2n + 1 to adjust center position to a tile center.
    /// </summary>
    /// <param name="mapSize">!Caution! : accepts only 2n + 1 size</param>
    /// <returns></returns>
    public Vector3 MiniMapCenterWorldPos(int mapSize = 15) => WorldPos(MiniMapCenterPos(mapSize));

    public bool[] ExportTileDiscoveredData()
    {
        var export = new bool[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                export[x + width * y] = discovered[x, y];
            }
        }
        return export;
    }

    public void ImportTileDiscoveredData(bool[] data)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                discovered[x, y] = data[x + width * y];
            }
        }
    }

    public static Color ConvertToColor(Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.Wall:
            case Terrain.Pillar:
            case Terrain.MessageWall:
            case Terrain.PictureWall:
            case Terrain.MessagePillar:
            case Terrain.BloodMessageWall:
            case Terrain.BloodMessagePillar:
                return Color.gray;

            case Terrain.Door:
            case Terrain.LockedDoor:
            case Terrain.OpenDoor:
            case Terrain.SealableDoor:
                return Color.red;

            case Terrain.ExitDoor:
            case Terrain.DownStairs:
            case Terrain.UpStairs:
                return Color.green;

            case Terrain.Box:
            case Terrain.Table:
            case Terrain.Chair:
            case Terrain.Cabinet:
            case Terrain.Bed:
            case Terrain.Fountain:
                return Color.blue;

            default:
                return Color.black;
        }
    }
}
