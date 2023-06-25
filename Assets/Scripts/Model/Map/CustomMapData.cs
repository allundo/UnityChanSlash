using System;
using System.Collections.Generic;

public class CustomMapData : IStairsData
{
    public int floor { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    public Terrain[,] matrix { get; private set; }
    public RawMapData rawMapData { get; private set; }

    public List<Pos> roomCenter { get; private set; } = new List<Pos>();
    public List<Pos> fixedMes { get; private set; } = new List<Pos>();
    public List<Pos> bloodMes { get; private set; } = new List<Pos>();
    public Pos upStairs { get; private set; }
    public Pos downStairs { get; private set; }
    public Pos exitDoor { get; private set; }

    public Dictionary<Pos, IDirection> deadEndPos { get; private set; } = null;
    public Dictionary<Pos, IDirection> fixedMessagePos { get; private set; } = null;
    public Dictionary<Pos, IDirection> bloodMessagePos { get; private set; } = null;

    private Dictionary<Terrain, Action<Pos>> dataMap;
    public CustomMapData(
        int floor,
        int[] customMapData,
        int width,
        Dictionary<Pos, IDirection> deadEndPos = null,
        Dictionary<Pos, IDirection> fixedMessagePos = null,
        Dictionary<Pos, IDirection> bloodMessagePos = null
    )
    {
        this.deadEndPos = deadEndPos;
        this.fixedMessagePos = fixedMessagePos;
        this.bloodMessagePos = bloodMessagePos;

        rawMapData = RawMapData.Convert(customMapData, width);

        this.floor = floor;
        this.width = width;
        this.height = rawMapData.height;

        matrix = new Terrain[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                matrix[i, j] = Util.ConvertTo<Terrain>(customMapData[i + j * width]);

                // Retrieve info from custom map data
                switch (matrix[i, j])
                {
                    case Terrain.RoomCenter:
                        roomCenter.Add(new Pos(i, j));
                        matrix[i, j] = Terrain.Ground;
                        break;

                    case Terrain.MessageWall:
                    case Terrain.MessagePillar:
                        fixedMes.Add(new Pos(i, j));
                        break;

                    case Terrain.BloodMessageWall:
                    case Terrain.BloodMessagePillar:
                        bloodMes.Add(new Pos(i, j));
                        break;

                    case Terrain.DownStairs:
                        if (floor == GameInfo.Instance.LastFloor)
                        {
                            matrix[i, j] = Terrain.Path;
                            break;
                        }
                        downStairs = new Pos(i, j);
                        break;

                    case Terrain.UpStairs:
                        if (floor == 1)
                        {
                            matrix[i, j] = Terrain.Path;
                            break;
                        }
                        upStairs = new Pos(i, j);
                        break;

                    case Terrain.ExitDoor:
                        exitDoor = new Pos(i, j);
                        break;
                }
            }
        }
    }

}