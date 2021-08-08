using UnityEngine;
using DG.Tweening;
using UniRx;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private Transform playerTransform = default;
    [SerializeField] private HidePool hidePool = default;
    [SerializeField] private PlayerCommander commander = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private UIPosition uiPosition = default;
    [SerializeField] private ThirdPersonCamera mainCamera = default;
    [SerializeField] private ScreenRotateHandler rotate = default;

    private bool isInitialOrientation = true;
    private RectTransform rtFadeScreen;

    public WorldMap worldMap { get; protected set; }

    public void Pause() { Time.timeScale = 0f; }
    public void Resume() { Time.timeScale = 1f; }

    protected override void Awake()
    {
        base.Awake();

        rtFadeScreen = fade.GetComponent<RectTransform>();

        var maze = new MazeCreator();
        maze.CreateMaze();

        worldMap = new WorldMap(maze);

        MapRenderer.Instance.Init(worldMap);
        MapRenderer.Instance.Fix(maze);

        placeEnemyGenerator.Place(worldMap);

        DOTween.SetTweensCapacity(500, 100);
    }

    void Start()
    {
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);

        fade.FadeIn(0.8f, 1.2f).Play();
        commander.EnqueueDropFloor();
        commander.EnqueueStartMessage();
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        rtFadeScreen.sizeDelta = new Vector2(Screen.width, Screen.height);

        uiPosition.ResetPosition(orientation);

        if (!isInitialOrientation)
        {
            hidePool.ReformHidePlates(orientation);
            mainCamera.ResetRenderSettings(orientation);
        }

        isInitialOrientation = false;
    }

    public Pos PlayerPos => worldMap.MapPos(playerTransform.position);
    public bool IsOnPlayer(Pos pos) => playerTransform.gameObject.activeSelf && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));

    public void EnterStair(bool isUpStair) => Debug.Log("Stair" + (isUpStair ? "UP" : "DOWN"));

    // FIXME
    public Pos GetPlayerInitPos => worldMap.InitPos;

}
