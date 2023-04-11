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

    private SpawnHandler spawnHandler;

    // Player info
    private HidePlateHandler hidePlateHandler;
    private PlayerInput input;
    private PlayerAnimator anim;
    private Collider playerCollider;
    private PlayerMapUtil playerMap;
    private PlayerReactor playerReact;

    private bool isInitialOrientation = true;

    public WorldMap worldMap { get; protected set; }
    private ResourceFX resourceFX;

    public IObservable<Unit> ExitObservable => exitSubject.IgnoreElements();
    private ISubject<Unit> exitSubject = new Subject<Unit>();

    public void PlayVFX(VFXType type, Vector3 pos) => resourceFX.PlayVFX(type, pos);
    public void PlaySnd(SNDType type, Vector3 pos) => resourceFX.PlaySnd(type, pos);

    protected override void Awake()
    {
        base.Awake();

        input = player.GetComponent<PlayerInput>();
        anim = player.GetComponent<PlayerAnimator>();
        playerMap = player.GetComponent<PlayerMapUtil>();
        playerReact = player.GetComponent<PlayerReactor>();
        playerCollider = player.GetComponent<Collider>();
        hidePlateHandler = player.GetComponent<HidePlateHandler>();

        worldMap = GameInfo.Instance.Map(0);
        mapRenderer.Render(worldMap);

        resourceFX = new ResourceFX(transform);
        spawnHandler = SpawnHandler.Instance;
    }

    private void StartSequence(IEnumerator startProcessCoroutine)
    {
        input.SetInputVisible(false);
        cover.SetAlpha(1f);

        InitPlayerPos();

        playerCollider.enabled = false;

        StartCoroutine(startProcessCoroutine);
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void DropStart() => StartSequence(DropStartWithDelay());

    private IEnumerator DropStartWithDelay(float delay = 0.6f)
    {
        yield return new WaitForSeconds(delay);

        spawnHandler.PlaceEnemyGenerators();
        yield return new WaitForEndOfFrame();

        playerCollider.enabled = true;
        yield return new WaitForEndOfFrame(); // Wait for PlayerAnimator.Start() and ItemGenerator.Start()

        cover.FadeIn(1.5f, 0.6f, false);

        eventManager.EventInit(worldMap);
        eventManager.InvokeGameEvent(0);
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void Restart() => StartSequence(RestartWithDelay());

    private IEnumerator RestartWithDelay(float delay = 0.2f)
    {
        yield return new WaitForSeconds(delay);

        spawnHandler.PlaceEnemyGenerators();

        yield return new WaitForEndOfFrame();

        playerCollider.enabled = true;
        yield return new WaitForEndOfFrame(); // Wait for PlayerAnimator.Start() and ItemGenerator.Start()

        cover.FadeIn(1f, 0.3f, false);

        eventManager.EventInit(worldMap);
        eventManager.InvokeGameEvent(1);
    }

    public void LoadDataStart()
    {
        cover.SetAlpha(1f);

        var dataStoreAgent = DataStoreAgent.Instance;

        playerCollider.enabled = false;
        dataStoreAgent.RespawnByGameData(worldMap);

        try
        {
            StartCoroutine(LoadStartCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError("インポートデータのロード中にエラー発生: " + e.Message);
            if (!Debug.isDebugBuild) dataStoreAgent.DeleteSaveDataFile();
            Debug.Log(e.StackTrace);
            throw e;
        }
    }

    private IEnumerator LoadStartCoroutine(float delay = 0.5f)
    {
        TimeManager.Instance.Pause();                       // Set pause to disable enemies' moving

        // Wait for Start() method calls of GameObjects
        yield return null;

        mapRenderer.ApplyTileOpen(worldMap);
        spawnHandler.PlaceEnemyGenerators();
        DataStoreAgent.Instance.RestorePlayerStatus();

        mainCamera.SwitchFloor(worldMap.floor);
        playerReact.SwitchFloor(worldMap.floor);

        playerCollider.enabled = true;

        yield return null;

        // Initialize hide plates after ApplyTileOpen();
        hidePlateHandler.Init();

        cover.FadeIn(1f, 0.5f, false);
        DOVirtual.DelayedCall(1f, () =>
        {
            TimeManager.Instance.Resume(false);         // Resume in the middle of fade-in
            eventManager.RestartCurrentEvent();         // Restart the event if it was in progress at exit
        }).Play();

        yield return null;
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

        eventManager.EventInit(worldMap);
    }

    public void DebugStartFloor(int floor)
    {
        Debug.Log("DEBUG MODE: floor = " + floor);

        if (floor == 2) spawnHandler.ActivateDebugEnemyGenerators();

        spawnHandler.PlaceEnemyGenerators();

        PlayerInfo.Instance.DebugSetLevel((floor - 1) * 2);
    }

    private void InitPlayerPos()
    {
        // Need to set player position before initialize HidePlateHandler.
        // MiniMap controlled by HidePlateHandler refers to player position and direction.
        playerMap.SetFloorStartPos(worldMap);
        hidePlateHandler.Init();

        // Initialize the point light position.
        // The point light refers to player position but need to be fixed at Camera looking position before DropStart().
        mainCamera.SwitchFloor(worldMap.floor);
        playerReact.SwitchFloor(worldMap.floor);
    }

    void Start()
    {
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);
        DataStoreAgent.Instance.EnableSave();
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

        HologramFade.isHologramOn = orientation == DeviceOrientation.LandscapeRight;
    }

    public void EnterStair(bool isDownStairs)
    {
        input.ClearAll();
        input.SetInputVisible(false);
        anim.Pause();
        mainCamera.StopScreen(cover.transform.GetSiblingIndex());

        cover.CoverOnObservable(1f, Observable.FromCoroutine(() => MoveFloor(isDownStairs)))
            .IgnoreElements()
            .Subscribe(null, StartFloor);
    }

    private void StartFloor()
    {
        DataStoreAgent.Instance.EnableSave();

        hidePlateHandler.OnStartFloor();
        mainCamera.ResetCrossFade();

        playerCollider.enabled = true;
        cover.FadeIn(1f, 0f, false);
        input.ValidateInput();
        input.SetInputVisible(true);
        anim.Resume();
    }

    private IEnumerator MoveFloor(bool isDownStairs)
    {
        var dataStoreAgent = DataStoreAgent.Instance;
        dataStoreAgent.DisableSave();

        // Save enemy respawn data
        // Store tile open data
        dataStoreAgent.SaveOnMovingFloor(isDownStairs);

        hidePlateHandler.OnMoveFloor();

        // Forbid enemies to access WorldMap
        spawnHandler.DisableAllEnemiesInput();
        EnemyCommand.ClearResetTweens();

        PlaySnd(SNDType.FloorMove, worldMap.WorldPos(worldMap.StairsEnter(isDownStairs).Key));

        yield return new WaitForEndOfFrame();

        // Wait for screenshot is applied to forefront Image
        yield return new WaitForEndOfFrame();

        // GameInfo.currentFloor is set to next floor
        var nextFloorMap = GameInfo.Instance.NextFloorMap(isDownStairs);

        playerCollider.enabled = false;
        playerMap.SetFloorStartPos(nextFloorMap, isDownStairs);
        hidePlateHandler.SwitchWorldMap(nextFloorMap);
        playerReact.SwitchFloor(nextFloorMap.floor);
        mainCamera.SwitchFloor(nextFloorMap.floor);
        yield return new WaitForEndOfFrame();

        // Clear character on tile info just before delete all enemies.
        worldMap.ClearCharacterOnTileInfo();

        // Stored floor enemies are respawn in this method.
        spawnHandler.DestroyCharacters();
        yield return new WaitForSeconds(0.5f);

        eventManager.SwitchWorldMap(nextFloorMap);
        yield return new WaitForEndOfFrame();

        mapRenderer.SetActiveTerrains(false);
        yield return new WaitForEndOfFrame();

        mapRenderer.DestroyObjects();
        yield return new WaitForEndOfFrame();

        mapRenderer.LoadFloorMaterials(nextFloorMap); // Switch world map for MapRenderer
        yield return new WaitForEndOfFrame();

        mapRenderer.InitMeshes();
        yield return new WaitForEndOfFrame();

        var terrainMeshes = mapRenderer.SetUpTerrainMeshes(nextFloorMap);
        yield return new WaitForSeconds(0.5f);

        mapRenderer.GenerateTerrain(terrainMeshes);
        yield return new WaitForEndOfFrame();

        mapRenderer.SwitchTerrainMaterials(nextFloorMap);
        yield return new WaitForEndOfFrame();

        mapRenderer.SetActiveTerrains(true);
        yield return new WaitForEndOfFrame();

        mapRenderer.ApplyTileOpen(nextFloorMap);
        yield return new WaitForEndOfFrame();

        spawnHandler.MoveFloorItems(nextFloorMap); // Switch world map for ItemGenerator
        yield return new WaitForEndOfFrame();

        // Update WorldMap just before respawn enemies. EnemyMapUtil refers to this "worldMap" on spawn.
        worldMap = nextFloorMap;
        spawnHandler.MoveFloorEnemies(nextFloorMap); // Switch world map for PlaceEnemyGenerator
        yield return new WaitForEndOfFrame();

        spawnHandler.PlaceEnemyGenerators();
        spawnHandler.RespawnWitch();
        yield return new WaitForEndOfFrame();
    }

    public void Exit()
    {
        var dataStoreAgent = DataStoreAgent.Instance;
        dataStoreAgent.DisableSave();

        var tm = TimeManager.Instance;

        tm.Pause(true);

        var gameInfo = GameInfo.Instance;

        var status = player.GetComponent<PlayerStatus>();

        gameInfo.TotalClearCounts(
            status.counter,
            tm.elapsedTimeSec,
            ItemInventory.Instance.SumUpPrices()
        );

        (gameInfo.strength, gameInfo.magic) = status.GetAttackMagic();

        var result = new ResultBonus(gameInfo);

        gameInfo.clearRecord = new DataStoreAgent.ClearRecord(gameInfo.title, result.wagesAmount, gameInfo.clearTimeSec, gameInfo.defeatCount);
        gameInfo.clearRank = dataStoreAgent.SaveClearRecords(gameInfo.clearRecord);

        cover.color = new Color(1f, 1f, 1f, 0f);
        cover.FadeOutObservable(3f, 0f, Ease.InCubic)
            .IgnoreElements()
            .Subscribe(null, exitSubject.OnCompleted)
            .AddTo(this);
    }

    public DataStoreAgent.EventData[] ExportEventData() => eventManager.ExportEventData();
    public int GetCurrentEvent() => eventManager.currentEvent;

    public void ImportRespawnData(DataStoreAgent.SaveData import) => eventManager.RespawnGameEvents(import.currentEvent, import.eventData, worldMap);
}
