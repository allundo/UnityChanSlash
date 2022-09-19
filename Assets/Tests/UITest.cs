using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;
using UniRx;

public class UITest
{
    private ResourceLoader resourceLoader;

    private ItemInventoryTest prefabItemInventory;
    private ItemIconGenerator itemIconGenerator;

    private FightCircleTest prefabFightCircle;
    private AttackInputControllerTest prefabAttackInput;
    private ForwardUI prefabForwardUI;
    private EnemyStatus prefabEnemyStatus;

    private GameObject testCanvas;
    private Camera mainCamera;
    private GameObject eventSystem;
    private DataStoreAgentTest dataStoreAgent;
    private GameInfo gameInfo;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));

        // Load from test resources
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));

        prefabItemInventory = Resources.Load<ItemInventoryTest>("Prefabs/UI/Item/ItemInventoryTest");
        itemIconGenerator = Object.Instantiate(Resources.Load<ItemIconGenerator>("Prefabs/UI/Item/ItemIconGenerator"));

        prefabFightCircle = Resources.Load<FightCircleTest>("Prefabs/UI/Fight/FightCircleTest");
        prefabAttackInput = Resources.Load<AttackInputControllerTest>("Prefabs/UI/Fight/AttackInputUITest");
        prefabForwardUI = Resources.Load<ForwardUI>("Prefabs/UI/Move/Forward");
        prefabEnemyStatus = Resources.Load<EnemyStatus>("Prefabs/TestEnemyStatus");

        dataStoreAgent = UnityEngine.Object.Instantiate(Resources.Load<DataStoreAgentTest>("Prefabs/System/DataStoreAgentTest"), Vector3.zero, Quaternion.identity); ;
        dataStoreAgent.KeepSaveDataFiles();

        gameInfo = UnityEngine.Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"), Vector3.zero, Quaternion.identity); ;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        dataStoreAgent.RestoreSaveDataFiles();

        Object.Destroy(gameInfo.gameObject);
        Object.Destroy(dataStoreAgent.gameObject);
        Object.Destroy(itemIconGenerator.gameObject);
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(eventSystem);
        Object.Destroy(resourceLoader.gameObject);
    }

    private void InitItemInventory(ItemInventoryTest itemInventory)
    {
        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        RectTransform rectTf = itemInventory.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        Vector2 size = rectTf.sizeDelta;
        rectTf.anchorMin = rectTf.anchorMax = new Vector2(0f, 0.5f);
        rectTf.anchoredPosition = new Vector2(size.x, size.y + 280f) * 0.5f;

        itemIconGenerator.SetParent(itemInventory.transform);
    }

    private void DestroyItemIcons()
    {
        DOTween.KillAll();
        itemIconGenerator.DestroyAll();
    }

    private void InitAnchoredPosition(GameObject uiObject, Vector2 anchoredPosition)
    {
        RectTransform rectTf = uiObject.GetComponent<RectTransform>();
        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        rectTf.anchorMin = rectTf.anchorMax = new Vector2(0.5f, 0.5f);
        rectTf.anchoredPosition = anchoredPosition;
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
    }

    [UnityTest]
    public IEnumerator _001_ItemInventoryKeyBladeSetTest()
    {
        // Setup
        var itemInventory = Object.Instantiate(prefabItemInventory);
        InitItemInventory(itemInventory);

        yield return null;

        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.KeyBlade));

        yield return new WaitForSeconds(5f);

        Assert.True(itemInventory.hasKeyBlade());

        // TearDown
        itemIconGenerator.DestroyAll();
        Object.Destroy(itemInventory.gameObject);

        yield return null;
    }

    [UnityTest]
    public IEnumerator _002_ItemPriceSumUpTest()
    {
        // Setup
        const int MAX_ITEMS = 30;

        var itemInventory = Object.Instantiate(prefabItemInventory);
        InitItemInventory(itemInventory);

        yield return null;

        // When
        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.KeyBlade));
        yield return null;
        ulong price1 = itemInventory.SumUpPrices();

        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.Potion, 2));
        yield return null;

        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.Coin, 3));
        yield return null;
        ulong price2 = itemInventory.SumUpPrices();

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.Coin, 2));
            yield return null;
        }
        ulong price3 = itemInventory.SumUpPrices();
        yield return null;

        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.KeyBlade));
        yield return null;
        ulong price4 = itemInventory.SumUpPrices();

        var dummyCoinIcon = itemIconGenerator.Spawn(Vector2.zero, resourceLoader.ItemInfo(ItemType.Coin, 1)).SetIndex(2);
        dummyCoinIcon.Inactivate();
        yield return null;

        itemInventory.Remove(dummyCoinIcon);
        yield return null;
        ulong price5 = itemInventory.SumUpPrices();

        // Then
        Assert.AreEqual(100000, price1);
        Assert.AreEqual(2657100, price2);
        Assert.AreEqual(48681300, price3);
        Assert.AreEqual(48681300, price4);
        Assert.AreEqual(46124400, price5);

        // TearDown
        DestroyItemIcons();
        Object.Destroy(itemInventory.gameObject);

        yield return null;
    }

    [UnityTest]
    public IEnumerator _003_StoreAndRestoreItemInventoryItems()
    {
        // Setup
        const int MAX_ITEMS = 30;
        var items = new ItemInfo[MAX_ITEMS];

        var itemInventory = Object.Instantiate(prefabItemInventory);
        InitItemInventory(itemInventory);

        yield return null;

        // When
        items[0] = resourceLoader.ItemInfo(ItemType.KeyBlade);
        itemInventory.PickUp(items[0]);
        ulong price1 = itemInventory.SumUpPrices();

        yield return null;

        items[1] = resourceLoader.ItemInfo(ItemType.Potion);
        itemInventory.PickUp(items[1]);
        yield return null;

        items[2] = resourceLoader.ItemInfo(ItemType.FireRing);
        itemInventory.PickUp(items[2]);
        yield return null;

        for (int i = 3; i < MAX_ITEMS; i++)
        {
            items[i] = resourceLoader.ItemInfo(ItemType.Coin);
            itemInventory.PickUp(items[i]);
            yield return null;
        }
        yield return new WaitForSeconds(2f);

        itemInventory.Remove(2, 3);
        itemInventory.Remove(3, 5);
        itemInventory.Remove(4, 4);
        yield return new WaitForSeconds(2f);

        Assert.IsNull(itemInventory.GetItem(17));
        Assert.IsNull(itemInventory.GetItem(28));
        Assert.IsNull(itemInventory.GetItem(24));

        var export = itemInventory.ExportInventoryItems();

        var saveData = new DataStoreAgent.SaveData()
        {
            currentFloor = 1,
            inventoryItems = export.Clone() as DataStoreAgent.ItemInfo[],
            mapData = new DataStoreAgent.MapData[] { new DataStoreAgent.MapData(new WorldMap()), null, null, null, null },
        };

        dataStoreAgent.SaveEncryptedRecordTest(saveData, dataStoreAgent.SAVE_DATA_FILE_NAME);
        yield return null;

        DestroyItemIcons();
        Object.Destroy(itemInventory.gameObject);
        yield return null;

        dataStoreAgent.ImportGameData();
        var loadData = dataStoreAgent.LoadGameData();
        yield return null;

        itemInventory = Object.Instantiate(prefabItemInventory);
        InitItemInventory(itemInventory);

        itemInventory.ImportInventoryItems(loadData.inventoryItems);

        var export2 = itemInventory.ExportInventoryItems();
        // Then

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < export.Length; i++)
        {
            if (export[i] == null)
            {
                Assert.IsNull(export2[i]);
                continue;
            }
            Assert.AreEqual(export[i].itemType, export2[i].itemType);
            Assert.AreEqual(export[i].numOfItem, export2[i].numOfItem);
        }

        // TearDown
        DestroyItemIcons();
        Object.Destroy(itemInventory.gameObject);

        yield return null;
    }

    public class MockEnemy
    {
        public Transform transform { get; private set; }
        public Vector3 corePos => transform.position;

        public IReadOnlyReactiveProperty<float> Life { get; private set; } = new ReactiveProperty<float>(10f);
        public IReadOnlyReactiveProperty<float> LifeMax { get; private set; } = new ReactiveProperty<float>(10f);

        public MockEnemy() : this(Vector3.zero) { }

        public MockEnemy(Vector3 pos)
        {
            transform = new GameObject("MockEnemy").transform;
            transform.position = pos;
        }
    }

    [UnityTest]
    public IEnumerator _004_FightCircleDisplayTest()
    {
        var forwardUIRT = Object.Instantiate(prefabForwardUI).GetComponent<RectTransform>();
        var attackInput = Object.Instantiate(prefabAttackInput);
        var fightCircle = Object.Instantiate(prefabFightCircle).InjectModules(attackInput, forwardUIRT);

        InitAnchoredPosition(fightCircle.gameObject, Vector2.zero);
        InitAnchoredPosition(attackInput.gameObject, Vector2.zero);
        InitAnchoredPosition(forwardUIRT.gameObject, Vector2.zero);

        attackInput.SetDummyCamera(mainCamera);

        var testEnemyStatus = Object.Instantiate(prefabEnemyStatus);
        var tfEnemy = testEnemyStatus.transform;
        testEnemyStatus.InitParam(new EnemyParam() { enemyCore = new Vector3(0, 0.5f, 0) });

        yield return null;

        fightCircle.SetActive(true, testEnemyStatus);

        tfEnemy.DOMoveY(7.5f, 5f).Play();

        yield return new WaitForSeconds(5f);

        Object.Destroy(forwardUIRT.gameObject);
        Object.Destroy(attackInput.gameObject);
        Object.Destroy(fightCircle.gameObject);

        yield return null;
    }
}
