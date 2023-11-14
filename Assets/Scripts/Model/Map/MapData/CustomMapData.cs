using System.Collections.Generic;

public class CustomMapData : IStairsData
{
    public int floor { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    public Terrain[,] matrix { get; private set; }
    public RawMapData rawMapData { get; private set; }

    public int numOfPits { get; private set; }

    public List<Pos> roomCenter { get; private set; } = new List<Pos>();

    /// <summary>
    /// Random message boards list retrieved from custom Terrain matrix.<br />
    /// Load messages from FloorMessagesData.RandomMessages[] randomly.
    /// </summary>
    public List<Pos> randomMes { get; private set; } = new List<Pos>();

    /// <summary>
    /// Secret blood message boards list retrieved from custom Terrain matrix.<br />
    /// Load messages from SecretMessagesDataAsset.SecretMessageData[] randomly.
    /// </summary>
    public List<Pos> secretMes { get; private set; } = new List<Pos>();

    public Pos upStairs { get; private set; }
    public Pos downStairs { get; private set; }
    public Pos exitDoor { get; private set; }

    public Dictionary<Pos, IDirection> fixedItemPos { get; private set; }
    public HashSet<Pos> randomItemPos { get; private set; }

    /// <summary>
    /// Message boards list with fixed place and direction. <br />
    /// Load messages from FloorMessagesData.FixedMessages[] according to its Dictionary order.
    /// </summary>
    public Dictionary<Pos, IDirection> fixedMessagePos { get; private set; }

    /// <summary>
    /// Blood message boards list with fixed place and direction. <br />
    /// Load messages from FloorMessagesSource.bloodMessages[] according to its Dictionary order.
    /// </summary>
    public Dictionary<Pos, IDirection> bloodMessagePos { get; private set; }

    /// <summary>
    /// Custom structures list with fixed place and direction. <br />
    /// Load structures from FloorCustomStructure.prefabStructures[] according to its Dictionary order.
    /// </summary>
    public Dictionary<Pos, IDirection> customStructurePos { get; private set; }

    public Pos picturePos { get; private set; } = new Pos();

    public static CustomMapData RetrieveData(PitMessageMapData data)
    {
        var handler = new DirMapHandler(data);
        return new CustomMapData(data.floor, handler.ConvertMapData(), data.width);
    }

    public CustomMapData(
        int floor,
        int[] customMapData,
        int width,
        Dictionary<Pos, IDirection> fixedItemPos = null,
        List<Pos> randomItemPos = null,
        Dictionary<Pos, IDirection> fixedMessagePos = null,
        Dictionary<Pos, IDirection> bloodMessagePos = null,
        Dictionary<Pos, IDirection> customStructurePos = null
    )
    {
        this.fixedItemPos = fixedItemPos ?? new Dictionary<Pos, IDirection>();
        this.randomItemPos = new HashSet<Pos>(randomItemPos ?? new List<Pos>());
        this.fixedMessagePos = fixedMessagePos ?? new Dictionary<Pos, IDirection>();
        this.bloodMessagePos = bloodMessagePos ?? new Dictionary<Pos, IDirection>();
        this.customStructurePos = customStructurePos ?? new Dictionary<Pos, IDirection>();

        rawMapData = RawMapData.Convert(customMapData, width);

        this.floor = floor;
        this.width = width;
        this.height = rawMapData.height;

        this.numOfPits = 0;

        matrix = new Terrain[width, height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                matrix[i, j] = Util.ConvertTo<Terrain>(customMapData[i + j * width]);

                // Retrieve info from custom map data
                switch (matrix[i, j])
                {
                    case Terrain.Box:
                        this.randomItemPos.Add(new Pos(i, j));
                        break;

                    case Terrain.RoomCenter:
                        roomCenter.Add(new Pos(i, j));
                        matrix[i, j] = Terrain.Ground;
                        break;

                    case Terrain.MessageWall:
                    case Terrain.MessagePillar:
                        randomMes.Add(new Pos(i, j));
                        break;

                    case Terrain.BloodMessageWall:
                    case Terrain.BloodMessagePillar:
                        secretMes.Add(new Pos(i, j));
                        break;

                    case Terrain.PictureWall:
                        picturePos = new Pos(i, j);
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

                    case Terrain.Pit:
                        ++numOfPits;
                        break;
                }
            }
        }
    }
}
