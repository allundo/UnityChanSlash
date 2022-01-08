using UnityEngine;
using DG.Tweening;
using UniRx;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private MapRenderer mapRenderer = default;
    [SerializeField] private GameObject player = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private ItemGenerator itemGenerator = default;
    [SerializeField] private FireBallGenerator fireBallGenerator = default;
    [SerializeField] private CoverScreen cover = default;
    [SerializeField] private UIPosition uiPosition = default;
    [SerializeField] private ThirdPersonCamera mainCamera = default;
    [SerializeField] private ScreenRotateHandler rotate = default;
    [SerializeField] private DebugEnemyGenerator debugEnemyGenerator = default;

    // Player info
    private Transform playerTransform = default;
    private HidePlateHandler hidePlateHandler = default;
    private PlayerInput input = default;
    private PlayerStatus status = default;


    private bool isInitialOrientation = true;

    public bool isPaused { get; private set; } = false;
    public bool isScaled { get; private set; } = false;

    public WorldMap worldMap { get; protected set; }

    public FireBallGenerator GetFireBallGenerator => fireBallGenerator;

    public void Pause(bool isHideUIs = false)
    {
        if (isPaused) return;

        if (isHideUIs) HideUIs();
        Time.timeScale = 0f;

        isPaused = isScaled = true;
    }

    public void Resume()
    {
        if (!isScaled) return;

        DisplayUIs();
        Time.timeScale = 1f;

        isPaused = isScaled = false;
    }

    public void TimeScale(float scale = 4f)
    {
        if (isPaused) return;

        Time.timeScale = scale;

        isScaled = true;
    }

    private void HideUIs() { input.SetInputVisible(false); }
    private void DisplayUIs() { input.SetInputVisible(true); }

    protected override void Awake()
    {
        base.Awake();

        playerTransform = player.transform;
        input = player.GetComponent<PlayerInput>();
        status = player.GetComponent<PlayerStatus>();
        hidePlateHandler = player.GetComponent<HidePlateHandler>();

        worldMap = GameInfo.Instance.NextFloorMap();
        mapRenderer.Render(worldMap);
    }

    void Start()
    {
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);
    }

    public void DropStart()
    {
        placeEnemyGenerator.Place();

        status.SetPosition();
        hidePlateHandler.Init();

        cover.FadeIn(1.5f, 1.0f, false).Play();
        input.EnqueueDropFloor();
        input.EnqueueStartMessage(
            new MessageData
            {
                sentences = new string[]
                {
                    "[仮] (ここに開幕の説明が入ります)",
                    "[仮] ・・・メタすぎる！"
                },
                faces = new FaceID[]
                {
                    FaceID.DISATTRACT,
                    FaceID.ANGRY
                }
            }
        );
    }

    public void Restart()
    {
        placeEnemyGenerator.Place();

        status.SetPosition();
        hidePlateHandler.Init();

        cover.FadeIn(1.0f, 0, false).Play();
        input.EnqueueRestartMessage(
            new MessageData
            {
                sentences = new string[]
                {
                        "[仮] ・・・という夢だったのさ",
                        "[仮] なんも解決してないんだけどねっ！",
                },
                faces = new FaceID[]
                {
                        FaceID.SMILE,
                        FaceID.ANGRY
                }
            }
        );
    }

    public void DebugStart()
    {
        Debug.Log("DEBUG MODE");
        debugEnemyGenerator.gameObject.SetActive(true);

        status.SetPosition();
        hidePlateHandler.Init();

        cover.SetAlpha(0f);
        input.SetInputVisible(true);
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        cover.sizeDelta = new Vector2(Screen.width, Screen.height);

        uiPosition.ResetPosition(orientation);

        if (!isInitialOrientation)
        {
            hidePlateHandler.ReformHidePlates(orientation);
            mainCamera.ResetRenderSettings(orientation);
        }

        isInitialOrientation = false;
    }

    public Pos PlayerPos => worldMap.MapPos(playerTransform.position);
    public Vector3 PlayerWorldPos
        => new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);


    public bool IsOnPlayer(Pos pos) => playerTransform.gameObject.activeSelf && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));

    public void EnterStair(bool isDownStairs)
    {
        debugEnemyGenerator.DestroyAll();
        debugEnemyGenerator.gameObject.SetActive(false);

        worldMap = GameInfo.Instance.NextFloorMap(isDownStairs);

        mapRenderer.Render(worldMap);

        placeEnemyGenerator.SwitchWorldMap(worldMap);
        itemGenerator.SwitchWorldMap(worldMap);

        status.SetPosition(isDownStairs);
        hidePlateHandler.SwitchWorldMap(worldMap);

        input.ValidateInput();
        input.SetInputVisible(true);
    }
}
