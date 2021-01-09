public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public WorldMap worldMap { get; protected set; }
    protected override void Awake()
    {
        base.Awake();

        var maze = new MazeCreator();
        maze.CreateMaze();

        worldMap = new WorldMap(maze);

        MapRenderer.Instance.Init(worldMap);
        MapRenderer.Instance.Fix(maze);

    }
}
