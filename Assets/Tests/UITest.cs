using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class UITest
{
    private ResourceLoader resourceLoader;

    private ItemInventoryTest itemInventory;
    private ItemInventoryTest prefabItemInventory;
    private ItemIconGenerator itemIconGenerator;
    private GameObject testCanvas;
    private Dictionary<ItemType, ItemInfo> itemInfo;
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

        itemInfo = new ItemInfoLoader(ResourceLoader.Instance.itemData).LoadItemInfo();
        prefabItemInventory = Resources.Load<ItemInventoryTest>("Prefabs/UI/Item/ItemInventoryTest");
        itemIconGenerator = Object.Instantiate(Resources.Load<ItemIconGenerator>("Prefabs/UI/Item/ItemIconGenerator"));

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

    [SetUp]
    public void SetUp()
    {
        itemInventory = Object.Instantiate(prefabItemInventory);
        itemIconGenerator.SetParent(itemInventory.transform);
        SetItemInventoryPos();
    }

    private void SetItemInventoryPos()
    {
        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        RectTransform rectTf = itemInventory.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        Vector2 size = rectTf.sizeDelta;
        rectTf.anchorMin = rectTf.anchorMax = new Vector2(0f, 0.5f);
        rectTf.anchoredPosition = new Vector2(size.x, size.y + 280f) * 0.5f;
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();

        itemIconGenerator.DestroyAll();
        Object.Destroy(itemInventory.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_ItemInventoryKeyBladeSetTest()
    {
        yield return null;

        itemInventory.PickUp(itemInfo[ItemType.KeyBlade]);

        yield return new WaitForSeconds(5f);

        Assert.True(itemInventory.hasKeyBlade());
    }

    [UnityTest]
    public IEnumerator _002_ItemPriceSumUpTest()
    {
        // Setup
        const int MAX_ITEMS = 30;

        yield return null;

        // When
        itemInventory.PickUp(itemInfo[ItemType.KeyBlade]);
        ulong price1 = itemInventory.SumUpPrices();

        yield return null;

        itemInventory.PickUp(itemInfo[ItemType.Potion]);
        yield return null;

        itemInventory.PickUp(itemInfo[ItemType.Coin]);
        ulong price2 = itemInventory.SumUpPrices();
        yield return null;

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            itemInventory.PickUp(itemInfo[ItemType.Coin]);
            yield return null;
        }
        ulong price3 = itemInventory.SumUpPrices();
        yield return null;

        itemInventory.PickUp(itemInfo[ItemType.KeyBlade]);
        ulong price4 = itemInventory.SumUpPrices();
        yield return null;

        var dummyCoinIcon = itemIconGenerator.Spawn(Vector2.zero, itemInfo[ItemType.Coin]).SetIndex(2);
        dummyCoinIcon.Inactivate();

        itemInventory.Remove(dummyCoinIcon);
        ulong price5 = itemInventory.SumUpPrices();
        yield return null;

        // Then
        Assert.AreEqual(100000, price1);
        Assert.AreEqual(952400, price2);
        Assert.AreEqual(23964500, price3);
        Assert.AreEqual(23964500, price4);
        Assert.AreEqual(23112200, price5);

    }

    [UnityTest]
    public IEnumerator _003_StoreAndRestoreItemInventoryItems()
    {
        // Setup
        const int MAX_ITEMS = 30;
        var items = new ItemInfo[MAX_ITEMS];

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

        DOTween.KillAll();
        itemIconGenerator.DestroyAll();
        Object.Destroy(itemInventory.gameObject);
        yield return null;

        dataStoreAgent.ImportGameData();
        var loadData = dataStoreAgent.LoadGameData();
        yield return null;

        itemInventory = Object.Instantiate(prefabItemInventory);
        itemIconGenerator.SetParent(itemInventory.transform);
        SetItemInventoryPos();
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
    }
}
