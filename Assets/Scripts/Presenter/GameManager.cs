using UnityEngine;
using DG.Tweening;
using UniRx;
using System;
using System.Collections;

public class GameManager : SingletonComponent<IGameManager>, IGameManager
{
    [SerializeField] protected MapRenderer mapRenderer = default;
    [SerializeField] protected GameObject player = default;
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
    protected ResourceFX resourceFX;

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
        BGMManager.Instance.SwitchFloor(worldMap.floor);
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

        yield return new WaitForSeconds(1f);
        BGMManager.Instance.PlayFloorBGM();
    }

    /// <summary>
    /// One of the start processes called before Start()
    /// </summary>
    public void Restart() => StartSequence(RestartWithDelay());

    private IEnumerator RestartWithDelay(float delay = 0.2f)
    {
        BGMManager.Instance.PlayFloorBGM();

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
        input.SetInputVisible();
        BGMManager.Instance.PlayFloorBGM();

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

        mapRenderer.ApplyTileState();
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

        BGMManager.Instance.PlayFloorBGM();
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

    protected virtual void Start()
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

    public void RedrawPlates()
    {
        hidePlateHandler.Redraw();
    }

    public void DestructDoor(Vector3 pos, IDirection forceDir)
    {
        mapRenderer.DoorDestructionVFX(pos, forceDir);
        mainCamera.Amplify(0.8f);
    }

    public void PitDropFX(Vector3 pos)
    {
        PlayVFX(VFXType.PitDrop, pos);
        PlaySnd(SNDType.PitDrop, pos);
        mainCamera.Amplify(0.3f);
    }

    public void EnterStair(bool isDownStairs)
    {
        mainCamera.StopAmplify();

        input.ClearAll();
        input.SetInputVisible(false);
        anim.Pause();

        cover.CoverOnObservable(1f, Observable.FromCoroutine(() => MoveFloor(isDownStairs)))
            .IgnoreElements()
            .Subscribe(null, StartFloor);
    }

    private void StartFloor()
    {
        DataStoreAgent.Instance.EnableSave();

        BGMManager.Instance.PlayFloorBGM();

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
        yield return mainCamera.DisplayScreenShot(cover.transform.GetSiblingIndex());

        var dataStoreAgent = DataStoreAgent.Instance;
        dataStoreAgent.DisableSave();

        // Save enemy respawn data
        // Store tile open data
        dataStoreAgent.SaveOnMovingFloor(isDownStairs);

        hidePlateHandler.OnMoveFloor();

        // Stop enemy generating
        spawnHandler.DisableAllEnemyGenerators();

        PlaySnd(SNDType.FloorMove, worldMap.WorldPos(worldMap.StairsEnter(isDownStairs).Key));

        var waitForEndOfFrame = new WaitForEndOfFrame();
        var waitForHalfSecond = new WaitForSeconds(0.5f);
        yield return waitForEndOfFrame;

        // Stop all enemy behaviors
        spawnHandler.DisableEnemyBehaviorsAll();

        // GameInfo.currentFloor is set to next floor
        var nextFloorMap = GameInfo.Instance.NextFloorMap(isDownStairs);

        playerCollider.enabled = false;
        BGMManager.Instance.SwitchFloor(nextFloorMap.floor);
        playerMap.SetFloorStartPos(nextFloorMap, isDownStairs);
        hidePlateHandler.SwitchWorldMap(nextFloorMap.miniMapData);
        playerReact.SwitchFloor(nextFloorMap.floor);
        mainCamera.SwitchFloor(nextFloorMap.floor);
        anim.ResetToIdle();
        yield return waitForEndOfFrame;

        // Clear character on tile info just before delete all enemies.
        worldMap.tileStateHandler.ClearCharacterOnTileInfo();

        // Stored floor enemies are respawn in this method.
        spawnHandler.DestroyCharacters();
        yield return waitForHalfSecond;

        eventManager.SwitchWorldMap(nextFloorMap);
        yield return waitForEndOfFrame;

        mapRenderer.SetActiveTerrains(false);
        yield return waitForEndOfFrame;

        mapRenderer.DestroyObjects();
        yield return waitForEndOfFrame;

        mapRenderer.LoadFloorMaterials(nextFloorMap); // Switch world map for MapRenderer
        yield return waitForEndOfFrame;

        mapRenderer.InitMeshes();
        yield return waitForEndOfFrame;

        var terrainMeshes = mapRenderer.SetUpTerrainMeshes(nextFloorMap.dirMapHandler);
        yield return waitForHalfSecond;

        mapRenderer.GenerateTerrain(terrainMeshes);
        yield return waitForEndOfFrame;

        mapRenderer.SwitchTerrainMaterials();
        yield return waitForEndOfFrame;

        mapRenderer.SetActiveTerrains(true);
        yield return waitForEndOfFrame;

        mapRenderer.ApplyTileState();
        yield return waitForEndOfFrame;

        spawnHandler.MoveFloorItems(nextFloorMap); // Switch world map for ItemGenerator
        yield return waitForEndOfFrame;

        // Update WorldMap just before respawn enemies. EnemyMapUtil refers to this "worldMap" on spawn.
        worldMap = nextFloorMap;
        spawnHandler.MoveFloorEnemies(nextFloorMap); // Switch world map for PlaceEnemyGenerator
        yield return waitForEndOfFrame;

        spawnHandler.PlaceEnemyGenerators();
        spawnHandler.RespawnWitch();
        yield return waitForEndOfFrame;
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

        gameInfo.clearRecord = new DataStoreAgent.ClearRecord(gameInfo.title, result.wagesAmount, gameInfo.endTimeSec, gameInfo.defeatCount);
        gameInfo.clearRank = dataStoreAgent.SaveClearRecords(gameInfo.clearRecord);
        dataStoreAgent.SaveInfoRecord(gameInfo);

        cover.ExitFadeOut(3f)
            .Subscribe(null, exitSubject.OnCompleted)
            .AddTo(this);
    }

    public DataStoreAgent.EventData[] ExportEventData() => eventManager.ExportEventData();
    public int GetCurrentEvent() => eventManager.currentEvent;

    public void ImportRespawnData(DataStoreAgent.SaveData import) => eventManager.RespawnGameEvents(import.currentEvent, import.eventData, worldMap);
}
