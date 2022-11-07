using UnityEngine;
using UniRx;
using System.Linq;
using System;
using System.Collections.Generic;

public interface IItemIndexHandler
{
    ItemIcon GetItem(int index);
    Vector2 UIPos(int index);
    void ExpandNum(int index);
    void DeleteNum(int index);
    bool UpdateItemNum(ItemIcon itemIcon);
    void SetItem(int index, ItemIcon itemIcon, bool tweenMove = false);
    bool RemoveItem(ItemIcon itemIcon);
    bool hasKeyBlade();
    bool hasItem(ItemIcon itemIcon);
    ulong SumUpPrices();
}

public abstract class ItemIndexHandler : IItemIndexHandler
{
    public int WIDTH { get; protected set; }
    public int HEIGHT { get; protected set; }
    public int MAX_ITEMS { get; protected set; }

    protected IDisposable[] itemEmptyCheck;
    protected ItemPanel[] panels;

    protected Transform uiTf;
    protected ItemInventory inventory;
    protected RectTransform uiRT;

    protected Vector2 uiSize;
    public Vector2 uiOrigin { get; protected set; }
    protected Vector2 offsetOrigin;

    protected Vector2 panelUnit;
    protected Vector2 panelOffsetCenter;

    public IObservable<int> OnPress => Observable.Merge(panels.Select(panel => panel.OnPress));
    public IObservable<int> OnRelease => Observable.Merge(panels.Select(panel => panel.OnRelease));

    protected int currentSelected;

    public ItemIndexHandler(ItemInventory inventory, RectTransform uiRT, ItemPanel prefabItemPanel, int width, int height)
    {
        WIDTH = width;
        HEIGHT = height;
        MAX_ITEMS = width * height;

        this.inventory = inventory;

        this.uiRT = uiRT;
        this.uiTf = uiRT.transform;
        this.uiSize = uiRT.sizeDelta;
        UpdateOrigin();

        panelUnit = new Vector2(uiSize.x / width, uiSize.y / height);
        panelOffsetCenter = new Vector2(panelUnit.x, -panelUnit.y) * 0.5f;
        panels = Enumerable
            .Range(0, MAX_ITEMS)
            .Select(
                index => Util.Instantiate(prefabItemPanel, inventory.GetComponent<RectTransform>().transform, false)
                    .SetPos(UIPos(index))
                    .SetIndex(index)
            )
            .ToArray();

        itemEmptyCheck = Enumerable.Repeat<IDisposable>(null, MAX_ITEMS).ToArray();

        currentSelected = MAX_ITEMS;
    }

    public virtual void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                uiRT.anchorMin = uiRT.anchorMax = new Vector2(0f, 0.5f);
                uiRT.anchoredPosition = new Vector2(uiSize.x, uiSize.y + 280f) * 0.5f;
                break;

            case DeviceOrientation.LandscapeRight:
                uiRT.anchorMin = uiRT.anchorMax = new Vector2(1f, 0f);
                uiRT.anchoredPosition = new Vector2(-(uiSize.x + 40f + Screen.width * ThirdPersonCamera.Margin), uiSize.y + 40f) * 0.5f;
                break;
        }

        UpdateOrigin();
    }

    protected void UpdateOrigin()
    {
        uiOrigin = new Vector2(uiRT.position.x - uiSize.x * 0.5f, uiRT.position.y + uiSize.y * 0.5f);
        offsetOrigin = GetOffsetOrigin();
    }

    protected virtual Vector2 GetOffsetOrigin() => Vector2.zero;

    public void SetEnablePanels(bool isEnable) => panels.ForEach(panel => panel.SetEnabled(isEnable));

    public Vector2 ConvertToVec(Vector2 screenPos) => screenPos - uiOrigin;
    public bool IsOnUI(Vector2 uiPos) => uiPos.x >= 0f && uiPos.x <= uiSize.x && uiPos.y <= 0f && uiPos.y >= -uiSize.y;

    protected virtual Vector2 LocalUIPos(int index) => LocalUIPos(index % WIDTH, index / WIDTH);
    protected virtual Vector2 LocalUIPos(int x, int y) => panelOffsetCenter + new Vector2(panelUnit.x * x, -panelUnit.y * y);

    public Vector2 UIPos(int index) => offsetOrigin + LocalUIPos(index);

    public ItemIcon GetItem(int index) => index < MAX_ITEMS ? Items[index] : null;
    protected abstract ItemIcon[] Items { get; }

    public virtual void SetItem(int index, ItemIcon itemIcon, bool tweenMove = false)
        => SetItemWithEmptyCheck(index, itemIcon, tweenMove);

    protected virtual void StoreItem(int index, ItemIcon itemIcon)
    {
        itemIcon?.SetInventoryType(false);
        Items[index] = itemIcon;
    }

    protected void SetItemWithEmptyCheck(int index, ItemIcon itemIcon, bool tweenMove = false)
    {
        StoreItem(index, itemIcon);

        itemEmptyCheck[index]?.Dispose();

        if (itemIcon != null)
        {
            if (tweenMove) itemIcon.MoveExclusive(UIPos(index));

            itemIcon.SetIndex(index);
            UpdateItemNum(itemIcon);

            itemEmptyCheck[index] = itemIcon.OnItemEmpty
                .Subscribe(_ => RemoveItem(itemIcon))
                .AddTo(itemIcon);
        }
        else
        {
            panels[index].SetItemNum(0);
            itemEmptyCheck[index] = null;
        }
    }

    public bool RemoveItem(ItemIcon itemIcon)
    {
        if (itemIcon == null) return false;

        itemEmptyCheck[itemIcon.index]?.Dispose();
        itemEmptyCheck[itemIcon.index] = null;

        itemIcon.Inactivate();
        SetItem(itemIcon.index, null);
        panels[itemIcon.index].SetItemNum(0);

        return true;
    }

    public bool UpdateItemNum(ItemIcon itemIcon)
    {
        if (GetItem(itemIcon.index) == null) return false;
        panels[itemIcon.index].SetItemNum(itemIcon.itemInfo.numOfItem);
        return true;
    }

    public void ExpandNum(int index)
    {
        if (currentSelected < MAX_ITEMS) panels[currentSelected].ShrinkNum();
        if (index < MAX_ITEMS) panels[index].ExpandNum(uiTf);
        currentSelected = index;
    }

    public void DeleteNum(int index)
    {
        if (index < MAX_ITEMS) panels[index].SetItemNum(0);
    }

    public void ForEach(Action<ItemIcon> action)
    {
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            action(GetItem(i));
        }
    }

    public T[] Select<T>(Func<ItemIcon, T> func)
    {
        T[] ret = new T[MAX_ITEMS];

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            ret[i] = func(GetItem(i));
        }

        return ret;
    }

    protected ItemIcon[] Where(Func<ItemIcon, bool> func)
    {
        var icons = new List<ItemIcon>();

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            var icon = GetItem(i);
            if (func(icon)) icons.Add(icon);
        }

        return icons.ToArray();
    }

    protected bool Any(Func<ItemIcon, bool> func)
    {
        return Where(itemIcon => itemIcon != null && func(itemIcon)).Count() > 0;
    }

    public bool hasKeyBlade()
    {
        return Any(itemIcon => itemIcon.itemInfo.type == ItemType.KeyBlade);
    }

    public bool hasItem(ItemIcon compareSrc)
    {
        return Any(itemIcon => itemIcon == compareSrc);
    }

    public ulong SumUpPrices()
    {
        return (ulong)Where(itemIcon => itemIcon != null).Sum(itemIcon => itemIcon.itemInfo.Price);
    }

    public DataStoreAgent.ItemInfo[] ExportAllItemInfo()
        => Items.Select(icon => icon == null ? null : new DataStoreAgent.ItemInfo(icon.itemInfo.type, icon.itemInfo.numOfItem)).ToArray();
}
