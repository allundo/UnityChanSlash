using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private Transform playerTransform = default;

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

    public Pos PlayerPos => worldMap.MapPos(playerTransform.position);
    public bool IsOnPlayer(Pos pos) => PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));
}
