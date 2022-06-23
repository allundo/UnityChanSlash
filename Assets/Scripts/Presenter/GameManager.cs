using UnityEngine;
using DG.Tweening;
using UniRx;
using System;
using System.Collections;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private MapRenderer mapRenderer = default;
    [SerializeField] private GameObject player = default;
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private ItemGenerator itemGenerator = default;
    [SerializeField] private BulletGeneratorLoader bulletGeneratorLoader = default;
    [SerializeField] private WitchLightGenerator lightGenerator = default;
    [SerializeField] private EventManager eventManager = default;
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

    private double elapsedTimeSec = 0;

    public WorldMap worldMap { get; protected set; }
    private ResourceFX resourceFX;

    public IObservable<Unit> ExitObservable => exitSubject.IgnoreElements();
    private ISubject<Unit> exitSubject = new Subject<Unit>();

    public BulletGenerator GetBulletGenerator(BulletType type) => bulletGeneratorLoader.bulletGenerators[type];

    public void SpawnLight(Vector3 pos) => lightGenerator.Spawn(pos);
    public void DistributeLight(Vector3 pos, float range) => lightGenerator.Spawn(pos + UnityEngine.Random.insideUnitSphere * range);

    public IEnemyStatus PlaceEnemy(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
        => placeEnemyGenerator.ManualSpawn(type, pos, dir, option, life);

    public IEnemyStatus PlaceEnemyRandom(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
        => placeEnemyGenerator.RandomSpawn(pos, dir, option, life);

    public IEnemyStatus PlaceWitch(Pos pos, IDirection dir, float waitFrames = 120f)
        => placeEnemyGenerator.SpawnWitch(pos, dir, waitFrames);

    public void PlayVFX(VFXType type, Vector3 pos) => resourceFX.PlayVFX(type, pos);
    public void PlaySnd(SNDType type, Vector3 pos) => resourceFX.PlaySnd(type, pos);

    public void EraseAllEnemies() => placeEnemyGenerator.EraseAllEnemies();

    public void Pause(bool isHideUIs = false)
    {
        if (isPaused) return;

        if (isHideUIs) input.SetInputVisible(false);
        Time.timeScale = 0f;

        isPaused = isScaled = true;
    }

    public void Resume(bool isShowUIs = true)
    {
        if (!isScaled) return;

        if (isShowUIs) input.SetInputVisible(true);
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

        worldMap = GameInfo.Instance.Map(0);
        mapRenderer.Render(worldMap);

        resourceFX = new ResourceFX();
    }

    void Start()
    {
        elapsedTimeSec = 0;
        eventManager.EventInit(worldMap);
        rotate.Orientation.Subscribe(orientation => ResetOrientation(orientation)).AddTo(this);
    }

    void Update()
    {
        elapsedTimeSec += Time.deltaTime;
    }

    public void DropStart()
    {
        input.SetInputVisible(false);
        cover.SetAlpha(1f);

        // Need to set player position before initialize HidePlateHandler.
        // MiniMap controlled by HidePlateHandler refers to player position and direction.
        map.SetPosition(worldMap);
        hidePlateHandler.Init();
        mainCamera.SwitchFloor(worldMap.floor);

        player.SetActive(false);

        StartCoroutine(DropStartWithDelay());
    }

    private IEnumerator DropStartWithDelay(float delay = 0.6f)
    {
        yield return new WaitForSeconds(delay);

        placeEnemyGenerator.Place();
        yield return new WaitForEndOfFrame();

        player.SetActive(true);
        yield return new WaitForEndOfFrame(); // Wait for PlayerAnimator.Start()

        cover.FadeIn(1.5f, 0.6f, false).Play();
        eventManager.DropStartEvent();
    }

    public void Restart()
    {
        placeEnemyGenerator.Place();

        map.SetPosition(worldMap);
        hidePlateHandler.Init();
        mainCamera.SwitchFloor(worldMap.floor);

        cover.FadeIn(1f, 0.5f, false).Play();

        eventManager.RestartEvent();
    }

    public void DebugStartFloor(int floor)
    {
        Debug.Log("DEBUG MODE: floor = " + floor);

        if (floor == 2) debugEnemyGenerators.ForEach(gen => gen.gameObject.SetActive(true));

        placeEnemyGenerator.Place();
    }

    public void DebugStart()
    {
        DebugStartFloor(GameInfo.Instance.currentFloor);

        map.SetPosition(worldMap);
        hidePlateHandler.Init();
        mainCamera.SwitchFloor(worldMap.floor);

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

    public Pos PlayerPos => map.onTilePos;
    public IDirection PlayerDir => map.dir;
    public bool IsPlayerHavingKeyBlade => input.GetItemInventory.hasKeyBlade();

    public Vector3 PlayerWorldPos
        => new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

    public bool IsOnPlayer(Pos pos) => playerTransform.gameObject.activeSelf && !map.isInPit && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));
    public bool IsOnPlayerTile(Pos pos) => playerTransform.gameObject.activeSelf && !map.isInPit && map.onTilePos == pos;
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

        // Deny enemies to access WorldMap
        debugEnemyGenerators.ForEach(gen => gen.DisableInputAll());
        placeEnemyGenerator.DisableAllEnemiesInput();

        worldMap = GameInfo.Instance.NextFloorMap(isDownStairs);
        yield return new WaitForEndOfFrame();

        // Wait for screenshot is applied to forefront Image
        yield return new WaitForEndOfFrame();

        map.SetPosition(worldMap, isDownStairs);
        hidePlateHandler.SwitchWorldMap(worldMap);
        mainCamera.SwitchFloor(worldMap.floor);
        yield return new WaitForEndOfFrame();

        placeEnemyGenerator.SwitchWorldMap(worldMap, map.onTilePos);

        debugEnemyGenerators.ForEach(gen =>
        {
            gen.DestroyAll();
            gen.gameObject.SetActive(false);
        });

        bulletGeneratorLoader.DestroyAll();

        lightGenerator.DestroyAll();

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

        itemGenerator.SwitchWorldMap(worldMap);
        itemGenerator.Turn(map.dir);
        yield return new WaitForEndOfFrame();

        placeEnemyGenerator.Place();
        placeEnemyGenerator.RespawnWitch();
        yield return new WaitForEndOfFrame();
    }

    public void Exit()
    {
        Pause(true);

        var gameInfo = GameInfo.Instance;

        gameInfo.moneyAmount = input.GetItemInventory.SumUpPrices();
        gameInfo.clearTimeSec = (ulong)elapsedTimeSec;
        gameInfo.SetMapComp();

        cover.color = new Color(1f, 1f, 1f, 0f);
        cover.FadeOut(3f).SetEase(Ease.InCubic)
            .OnComplete(exitSubject.OnCompleted)
            .Play();
    }
}
