using UnityEngine;
using DG.Tweening;
using UniRx;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private MapRenderer mapRenderer = default;
    [SerializeField] private Transform playerTransform = default;
    [SerializeField] private HidePool hidePool = default;
    [SerializeField] private PlayerCommander commander = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private CoverScreen cover = default;
    [SerializeField] private UIPosition uiPosition = default;
    [SerializeField] private ThirdPersonCamera mainCamera = default;
    [SerializeField] private ScreenRotateHandler rotate = default;

    private bool isInitialOrientation = true;

    public WorldMap worldMap { get; protected set; }

    public void Pause(bool isHideUIs = false)
    {
        if (isHideUIs) HideUIs();
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        DisplayUIs();
        Time.timeScale = 1f;
    }

    private void HideUIs() { commander.InvisibleInput(); }
    private void DisplayUIs() { commander.VisibleInput(); }

    protected override void Awake()
    {
        base.Awake();

        worldMap = GameInfo.Instance.Map(1);
        mapRenderer.Render(worldMap);
    }

    void Start()
    {
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);
        placeEnemyGenerator.Place(worldMap);

        DropStart();
    }

    public void DropStart()
    {
        cover.SetAlpha(1f);
        cover.FadeIn(1.0f, 1.1f).Play();
        commander.EnqueueDropFloor();
        commander.EnqueueStartMessage();
    }

    public void ReStart()
    {
        cover.SetAlpha(1f);
        cover.FadeIn(1.0f).Play();
        commander.EnqueueRestartMessage();
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        cover.sizeDelta = new Vector2(Screen.width, Screen.height);

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
