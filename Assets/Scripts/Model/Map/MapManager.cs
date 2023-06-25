using System.Linq;
using System.Collections.Generic;

public class MapManager
{
    public int floor { get; private set; } = 0;
    public int width { get; private set; }
    public int height { get; private set; }

    public List<Pos> roomCenterPos { get; private set; } = new List<Pos>();

    public PitMessageMapData pitMessageMapData { get; protected set; }
    public StairsMapData stairsMapData { get; protected set; }
    public DirMapHandler dirMapHandler { get; protected set; }

    public Terrain[,] matrix { get; protected set; }
    public Dir[,] dirMap { get; protected set; }

    // Create maze
    public MapManager(int floor, int width = 49, int height = 49)
    {
        this.floor = floor;
        this.width = width;
        this.height = height;

        var maze = new MazeCreator(width, height);

        roomCenterPos = new List<Pos>(maze.roomCenterPos);

        var dirMapData = new DirMapData(maze.matrix, width, height);

        dirMapHandler = new DirMapHandler(dirMapData);
        stairsMapData = new StairsMapData(dirMapData, floor);
        pitMessageMapData = new PitMessageMapData(stairsMapData, floor);

        matrix = stairsMapData.matrix;
        dirMap = stairsMapData.dirMap;
    }

    // Load from MapData
    public MapManager(int floor, DataStoreAgent.MapData mapData)
    {
        this.floor = floor;
        this.width = mapData.mapSize;
        this.height = mapData.mapMatrix.Length / width;
        this.roomCenterPos = mapData.roomCenterPos.ToList();

        var dirMapData = new DirMapData(mapData);
        dirMapHandler = new DirMapHandler(dirMapData);
        stairsMapData = new StairsMapData(dirMapData, mapData);
        pitMessageMapData = new PitMessageMapData(dirMapData);

        this.matrix = stairsMapData.matrix;
        this.dirMap = stairsMapData.dirMap;
    }

    // Custom map data with custom deadEndPos.
    public MapManager(CustomMapData data)
    {
        this.floor = data.floor;
        this.width = data.width;
        this.height = data.height;

        this.roomCenterPos = data.roomCenter;

        var dirMapData = new DirMapData(data);
        dirMapHandler = new DirMapHandler(dirMapData);
        stairsMapData = new StairsMapData(dirMapData, data);
        pitMessageMapData = new PitMessageMapData(dirMapHandler, data);

        matrix = stairsMapData.matrix;
        dirMap = stairsMapData.dirMap;
    }
}
