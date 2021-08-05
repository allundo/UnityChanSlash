using UnityEngine;
using DG.Tweening;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private Transform playerTransform = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private FadeScreen fade = default;

    public WorldMap worldMap { get; protected set; }

    protected override void Awake()
    {
        base.Awake();

        fade.FadeIn(0.8f).Play();

        var maze = new MazeCreator();
        maze.CreateMaze();

        worldMap = new WorldMap(maze);

        MapRenderer.Instance.Init(worldMap);
        MapRenderer.Instance.Fix(maze);

        placeEnemyGenerator.Place(worldMap);

        DOTween.SetTweensCapacity(500, 100);
    }

    public Pos PlayerPos => worldMap.MapPos(playerTransform.position);
    public bool IsOnPlayer(Pos pos) => playerTransform.gameObject.activeSelf && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));

    public void EnterStair(bool isUpStair) => Debug.Log("Stair" + (isUpStair ? "UP" : "DOWN"));

    // FIXME
    public Pos GetPlayerInitPos => worldMap.InitPos;

}
