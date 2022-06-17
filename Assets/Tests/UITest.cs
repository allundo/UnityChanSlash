using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class UITest
{
    private ResourceLoader resourceLoader;

    private ItemInventory itemInventory;
    private ItemInventory prefabItemInventory;
    private ItemIconGenerator itemIconGenerator;
    private GameObject testCanvas;
    private Dictionary<ItemType, ItemInfo> itemInfo;
    private Camera mainCamera;
    private GameObject eventSystem;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));

        // Load from test resources
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));

        itemInfo = new ItemInfoLoader(ResourceLoader.Instance.itemData).LoadItemInfo();
        prefabItemInventory = Resources.Load<ItemInventory>("Prefabs/UI/Item/ItemInventory");
        itemIconGenerator = Object.Instantiate(Resources.Load<ItemIconGenerator>("Prefabs/UI/Item/ItemIconGenerator"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
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

        Object.Destroy(itemInventory.gameObject);
        itemIconGenerator.DestroyAll();
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
}
