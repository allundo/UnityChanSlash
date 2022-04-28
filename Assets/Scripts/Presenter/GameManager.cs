using UnityEngine;
using DG.Tweening;
using UniRx;
using System.Collections;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private MapRenderer mapRenderer = default;
    [SerializeField] private GameObject player = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private ItemGenerator itemGenerator = default;
    [SerializeField] private BulletGeneratorLoader bulletGeneratorLoader = default;
    [SerializeField] private CoverScreen cover = default;
    [SerializeField] private UIPosition uiPosition = default;
    [SerializeField] private ThirdPersonCamera mainCamera = default;
    [SerializeField] private ScreenRotateHandler rotate = default;
    [SerializeField] private DebugEnemyGenerator[] debugEnemyGenerators = default;

    // Player info
    private Transform playerTransform = default;
    private HidePlateHandler hidePlateHandler = default;
    private PlayerInput input = default;
    private PlayerMapUtil map = default;

    private bool isInitialOrientation = true;

    public bool isPaused { get; private set; } = false;
    public bool isScaled { get; private set; } = false;

    public WorldMap worldMap { get; protected set; }

    public BulletGenerator GetBulletGenerator(BulletType type) => bulletGeneratorLoader.bulletGenerators[type];

    public void Pause(bool isHideUIs = false)
    {
        if (isPaused) return;

        if (isHideUIs) input.SetInputVisible(false);
        Time.timeScale = 0f;

        isPaused = isScaled = true;
    }

    public void Resume()
    {
        if (!isScaled) return;

        input.SetInputVisible(true);
        Time.timeScale = 1f;

        isPaused = isScaled = false;
    }

    public void TimeScale(float scale = 5f)
    {
        if (isPaused) return;

        Time.timeScale = scale;

        isScaled = true;
    }

    protected override void Awake()
    {
        base.Awake();

        playerTransform = player.transform;
        input = player.GetComponent<PlayerInput>();
        map = player.GetComponent<PlayerMapUtil>();
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
        input.SetInputVisible(false);
        cover.SetAlpha(1f);

        map.SetPosition(worldMap);
        hidePlateHandler.Init();

        player.SetActive(false);

        StartCoroutine(DropStartWithDelay());
    }

    private IEnumerator DropStartWithDelay(float delay = 0.6f)
    {
        yield return new WaitForSeconds(delay);

        placeEnemyGenerator.Place();
        yield return new WaitForEndOfFrame();

        player.SetActive(true);
        cover.FadeIn(1.5f, 0.6f, false).Play();
        input.EnqueueDropFloor();

        input.EnqueueStartMessage(
            new MessageData[]
            {
                new MessageData("いきなりなんなのさ・・・", FaceID.DISATTRACT),
                new MessageData("久々の出番なのに、扱いが雑じゃない！？", FaceID.ANGRY)
            },
            false
        );

        var exitDir = worldMap.exitDoorDir;

        if (map.dir.IsLeft(exitDir))
        {
            input.EnqueueTurnR();
        }
        else if (map.dir.IsRight(exitDir))
        {
            input.EnqueueTurnL();
        }
        else if (map.dir.IsSame(exitDir))
        {
            input.EnqueueTurnL();
            input.EnqueueTurnL();
        }

        input.EnqueueStartMessage(
            new MessageData[]
            {
                new MessageData("なんか使う標示まちがってる気がするけど", FaceID.DEFAULT),
                new MessageData("どうみてもこれが出口だね", FaceID.NOTICE),
                new MessageData("・・・うーん", FaceID.DISATTRACT),
                new MessageData("鍵が掛かってますねぇ！", FaceID.DISATTRACT),
            }
        );
    }

    public void Restart()
    {
        placeEnemyGenerator.Place();

        map.SetPosition(worldMap);
        hidePlateHandler.Init();

        cover.FadeIn(1f, 0.5f, false).Play();
        input.EnqueueRestartMessage(
            new MessageData[]
            {
                new MessageData("[仮] ・・・という夢だったのさ", FaceID.SMILE),
                new MessageData("[仮] なんも解決してないんだけどねっ！", FaceID.ANGRY)
            }
        );
    }

    public void DebugStart()
    {
        Debug.Log("DEBUG MODE");
        debugEnemyGenerators.ForEach(gen => gen.gameObject.SetActive(true));

        map.SetPosition(worldMap);
        hidePlateHandler.Init();

        cover.SetAlpha(0f);
        input.SetInputVisible(true);
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        cover.sizeDelta = new Vector2(Screen.width, Screen.height);

        uiPosition.ResetOrientation(orientation);

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
    public bool IsOnPlayerTile(Pos pos) => playerTransform.gameObject.activeSelf && map.onTilePos == pos;
    public bool IsOnPlayerTile(int x, int y) => IsOnPlayerTile(new Pos(x, y));

    public void EnterStair(bool isDownStairs)
    {
        input.ClearAll();
        input.SetInputVisible(false);
        mainCamera.StopScreen(cover.transform.GetSiblingIndex());

        Observable
            .Merge(
                cover.CoverOn().OnCompleteAsObservable().Select(t => Unit.Default),
                Observable.FromCoroutine(() => MoveFloor(isDownStairs))
            )
            .IgnoreElements()
            .Subscribe(null, StartFloor);
    }

    private void StartFloor()
    {
        hidePlateHandler.OnStartFloor();
        mainCamera.ResetCrossFade();

        cover.FadeIn(1f, 0f, false).Play();
        input.ValidateInput();
        input.SetInputVisible(true);
    }

    private IEnumerator MoveFloor(bool isDownStairs)
    {
        hidePlateHandler.OnMoveFloor();

        worldMap = GameInfo.Instance.NextFloorMap(isDownStairs);
        yield return new WaitForEndOfFrame();

        // Wait for screenshot is applied to forefront Image
        yield return new WaitForEndOfFrame();

        map.SetPosition(worldMap, isDownStairs);
        hidePlateHandler.SwitchWorldMap(worldMap);
        yield return new WaitForEndOfFrame();

        debugEnemyGenerators.ForEach(gen =>
        {
            gen.DestroyAll();
            gen.gameObject.SetActive(false);
        });

        placeEnemyGenerator.SwitchWorldMap(worldMap, map.onTilePos);
        bulletGeneratorLoader.DestroyAll();

        yield return new WaitForEndOfFrame();

        mapRenderer.StoreMapData();
        yield return new WaitForSeconds(0.5f);

        mapRenderer.SetActiveTerrains(false);
        yield return new WaitForEndOfFrame();

        mapRenderer.DestroyObjects();
        yield return new WaitForEndOfFrame();

        mapRenderer.LoadFloorMaterials(worldMap);
        yield return new WaitForEndOfFrame();

        mapRenderer.InitMeshes();
        yield return new WaitForEndOfFrame();

        var terrainMeshes = mapRenderer.SetUpTerrainMeshes(worldMap);
        yield return new WaitForSeconds(0.5f);

        mapRenderer.GenerateTerrain(terrainMeshes);
        yield return new WaitForEndOfFrame();

        mapRenderer.SwitchTerrainMaterials(worldMap);
        yield return new WaitForEndOfFrame();

        mapRenderer.SetActiveTerrains(true);
        yield return new WaitForEndOfFrame();

        mapRenderer.RestoreMapData(worldMap);
        yield return new WaitForEndOfFrame();

        itemGenerator.SwitchWorldMap(worldMap);
        itemGenerator.Turn(map.dir);
        yield return new WaitForEndOfFrame();
    }

    public void Exit()
    {
        Pause(true);
        cover.color = new Color(1f, 1f, 1f, 0f);
        cover.FadeOut(3f).SetEase(Ease.InCubic).Play();
    }
}
