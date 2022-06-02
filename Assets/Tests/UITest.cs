using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UITest
{
    [UnityTest]
    public IEnumerator _001_ItemInventoryKeyBladeSetTest()
    {
        Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));

        // Load from test resources
        var mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        var testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
        var itemInventory = Object.Instantiate(Resources.Load<ItemInventory>("Prefabs/UI/Item/ItemInventory"));
        var eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        RectTransform rectTf = itemInventory.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        Vector2 size = rectTf.sizeDelta;
        rectTf.anchorMin = rectTf.anchorMax = new Vector2(0f, 0.5f);
        rectTf.anchoredPosition = new Vector2(size.x, size.y + 280f) * 0.5f;

        yield return null;

        var itemInfo = new ItemInfoLoader(ResourceLoader.Instance.itemData).LoadItemInfo();
        itemInventory.PickUp(itemInfo[ItemType.KeyBlade]);

        yield return new WaitForSeconds(5f);

        Assert.True(itemInventory.hasKeyBlade());
    }
}
