using UnityEngine;
using DG.Tweening;
using UniRx;
using System;
using System.Collections;

public class GameManager : SingletonComponent<IGameManager>, IGameManager
{
    [SerializeField] private MapRenderer mapRenderer = default;
    [SerializeField] private GameObject player = default;
    [SerializeField] private EventManager eventManager = default;
    [SerializeField] private CoverScreen cover = default;
    [SerializeField] private ThirdPersonCamera mainCamera = default;
    [SerializeField] private ScreenRotateHandler rotate = default;

    /// <summary>
    /// Active message controller for notifications.
    /// </summary>
    [SerializeField] public ActiveMessageController activeMessageUI = default;

    private SpawnHandler spawnHandler;

    // Player info
    private HidePlateHandler hidePlateHandler;
    private PlayerInput input;
    private PlayerMapUtil map;

    private bool isInitialOrientation = true;

    public WorldMap worldMap { get; protected set; }
    private ResourceFX resourceFX;

    public IObservable<Unit> ExitObservable => exitSubject.IgnoreElements();
    private ISubject<Unit> exitSubject = new Subject<Unit>();

    public void PlayVFX(VFXType type, Vector3 pos) => resourceFX.PlayVFX(type, pos);
    public void PlaySnd(SNDType type, Vector3 pos) => resourceFX.PlaySnd(type, pos);

    public void ActiveMessage(string message) => ActiveMessage(new ActiveMessageData(message));
    public void ActiveMessage(ActiveMessageData data) => activeMessageUI.InputMessageData(data);

    protected override void Awake()
    {
        base.Awake();

        input = player.GetComponent<PlayerInput>();
        map = player.GetComponent<PlayerMapUtil>();
        hidePlateHandler = player.GetComponent<HidePlateHandler>();

        worldMap = GameInfo.Instance.Map(0);
        mapRenderer.Render(worldMap);

        resourceFX = new ResourceFX(transform);
        spawnHandler = SpawnHandler.Instance;
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void DropStart()
    {
        input.SetInputVisible(false);
        cover.SetAlpha(1f);

        InitPlayerPos();

        player.SetActive(false);

        StartCoroutine(DropStartWithDelay());
    }

    private IEnumerator DropStartWithDelay(float delay = 0.6f)
    {
        yield return new WaitForSeconds(delay);

        spawnHandler.PlaceEnemyGenerators();
        yield return new WaitForEndOfFrame();

        player.SetActive(true);
        yield return new WaitForEndOfFrame(); // Wait for PlayerAnimator.Start()

        cover.FadeIn(1.5f, 0.6f, false).Play();
        eventManager.DropStartEvent();
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void Restart()
    {
        spawnHandler.PlaceEnemyGenerators();

        InitPlayerPos();

        cover.FadeIn(1f, 0.5f, false).Play();

        eventManager.RestartEvent();
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void DebugStart()
    {
        DebugStartFloor(GameInfo.Instance.currentFloor);

        InitPlayerPos();

        cover.SetAlpha(0f);
        input.SetInputVisible(true);
    }

    public void DebugStartFloor(int floor)
    {
        Debug.Log("DEBUG MODE: floor = " + floor);

        if (floor == 2) spawnHandler.ActivateDebugEnemyGenerators();

        spawnHandler.PlaceEnemyGenerators();
    }

    private void InitPlayerPos()
    {
        // Need to set player position before initialize HidePlateHandler.
        // MiniMap controlled by HidePlateHandler refers to player position and direction.
        map.SetPosition(worldMap);
        hidePlateHandler.Init();

        // Initialize the point light position.
        // The point light refers to player position but need to be fixed at Camera looking position before DropStart().
        mainCamera.SwitchFloor(worldMap.floor);
    }

    void Start()
    {
        eventManager.EventInit(worldMap);
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        cover.sizeDelta = new Vector2(Screen.width, Screen.height);

        if (!isInitialOrientation)
        {
            hidePlateHandler.ReformHidePlates(orientation);
            mainCamera.ResetRenderSettings(orientation);
        }

        isInitialOrientation = false;
    }

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

        // Deny enemies to access WorldMap
        spawnHandler.DisableAllEnemiesInput();

        worldMap = GameInfo.Instance.NextFloorMap(isDownStairs);
        yield return new WaitForEndOfFrame();

        // Wait for screenshot is applied to forefront Image
        yield return new WaitForEndOfFrame();

        map.SetPosition(worldMap, isDownStairs);
        hidePlateHandler.SwitchWorldMap(worldMap);
        mainCamera.SwitchFloor(worldMap.floor);
        yield return new WaitForEndOfFrame();

        spawnHandler.MoveFloorCharacters(worldMap, map.onTilePos);
        eventManager.SwitchWorldMap(worldMap);

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

        spawnHandler.MoveFloorItems(worldMap, map.dir);
        yield return new WaitForEndOfFrame();

        spawnHandler.PlaceEnemyGenerators();
        spawnHandler.RespawnWitch();
        yield return new WaitForEndOfFrame();
    }

    public void Exit()
    {
        var tm = TimeManager.Instance;

        tm.Pause(true);

        var gameInfo = GameInfo.Instance;

        gameInfo.moneyAmount = input.GetItemInventory.SumUpPrices();
        gameInfo.clearTimeSec = tm.elapsedTimeSec;
        gameInfo.SetMapComp();

        cover.color = new Color(1f, 1f, 1f, 0f);
        cover.FadeOut(3f).SetEase(Ease.InCubic)
            .OnComplete(exitSubject.OnCompleted)
            .Play();
    }
}
