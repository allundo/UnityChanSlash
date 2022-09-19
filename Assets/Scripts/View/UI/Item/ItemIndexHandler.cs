using UnityEngine;
using UniRx;
using System.Linq;
using System;
using System.Collections.Generic;

public class ItemIndexHandler
{
    public int WIDTH { get; private set; }
    public int HEIGHT { get; private set; }
    public int MAX_ITEMS { get; private set; }

    private ItemIcon[] items;
    private IDisposable[] itemEmptyCheck;

    private ItemPanel[] panels;

    private Vector2 unit;
    private Vector2 origin;
    private Vector2 inventoryOrigin;
    private Transform inventoryTf;

    private Vector2 uiSize;

    public IObservable<int> OnPress => Observable.Merge(panels.Select(panel => panel.OnPress));
    public IObservable<int> OnRelease => Observable.Merge(panels.Select(panel => panel.OnRelease));

    private int currentSelected;
    public void ExpandNum(int index)
    {
        if (currentSelected < MAX_ITEMS) panels[currentSelected].ShrinkNum();
        if (index < MAX_ITEMS) panels[index].ExpandNum(inventoryTf);
        currentSelected = index;
    }

    public void DeleteNum(int index)
    {
        if (index < MAX_ITEMS) panels[index].SetItemNum(0);
    }

    public ItemIndexHandler(RectTransform rt, int width, int height)
    {
        this.WIDTH = width;
        this.HEIGHT = height;
        MAX_ITEMS = width * height;

        items = Enumerable.Repeat<ItemIcon>(null, MAX_ITEMS).ToArray();
        itemEmptyCheck = Enumerable.Repeat<IDisposable>(null, MAX_ITEMS).ToArray();

        uiSize = rt.sizeDelta;
        unit = new Vector2(uiSize.x / WIDTH, uiSize.y / HEIGHT);

        // Anchor of ItemIcon is set to left top by default on prefab
        origin = new Vector2(unit.x, -unit.y) * 0.5f;

        inventoryOrigin = new Vector2(rt.position.x - uiSize.x * 0.5f, rt.position.y + uiSize.y * 0.5f);
        inventoryTf = rt.transform;

        currentSelected = MAX_ITEMS;
    }

    public ItemIndexHandler SetPanels(ItemPanel prefabItemPanel)
    {
        panels = Enumerable
            .Range(0, MAX_ITEMS)
            .Select(
                index => Util.Instantiate(prefabItemPanel, inventoryTf, false)
                    .SetPos(UIPos(index))
                    .SetIndex(index)
            )
            .ToArray();

        return this;
    }

    public void SetEnablePanels(bool isEnable) => panels.ForEach(panel => panel.SetEnabled(isEnable));

    public Vector2 ConvertToVec(Vector2 screenPos) => screenPos - inventoryOrigin;
    public bool IsOnUI(Vector2 uiPos) => uiPos.x >= 0f && uiPos.x <= uiSize.x && uiPos.y <= 0f && uiPos.y >= -uiSize.y;

    public Vector2 UIPos(int index) => UIPos(Index(index));
    public Vector2 UIPos(int x, int y) => origin + new Vector2(unit.x * x, -unit.y * y);
    public Vector2 UIPos(Pos pos) => UIPos(pos.x, pos.y);

    public bool IsValidIndex(int x, int y)
        => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
    public bool IsValidIndex(Pos pos) => IsValidIndex(pos.x, pos.y);

    public int Index(Pos index) => index.x + WIDTH * index.y;
    public Pos Index(int index) => new Pos(index % WIDTH, index / WIDTH);

    public void SetItem(ItemIcon itemIcon) => SetItem(itemIcon.index, itemIcon);

    public void SetItem(int index, ItemIcon itemIcon = null)
    {
        items[index] = itemIcon;

        itemEmptyCheck[index]?.Dispose();

        if (itemIcon != null)
        {
            UpdateItemNum(itemIcon);

            itemEmptyCheck[index] = itemIcon.OnItemEmpty
                .Subscribe(_ => RemoveItem(items[index]))
                .AddTo(itemIcon);
        }
        else
        {
            panels[index].SetItemNum(0);
            itemEmptyCheck[index] = null;
        }
    }

    public bool UpdateItemNum(ItemIcon itemIcon)
    {
        if (items[itemIcon.index] == null) return false;
        panels[itemIcon.index].SetItemNum(itemIcon.itemInfo.numOfItem);
        return true;
    }

    public ItemIcon GetItem(Pos index) => GetItem(index.x, index.y);
    public ItemIcon GetItem(int x, int y) => IsValidIndex(x, y) ? items[x + WIDTH * y] : null;
    public ItemIcon GetItem(int index) => index < MAX_ITEMS ? items[index] : null;

    public bool RemoveItem(Pos index) => RemoveItem(index.x, index.y);
    public bool RemoveItem(int x, int y) => RemoveItem(GetItem(x, y));

    public bool RemoveItem(ItemIcon itemIcon)
    {
        if (itemIcon == null) return false;

        itemEmptyCheck[itemIcon.index]?.Dispose();
        itemEmptyCheck[itemIcon.index] = null;

        itemIcon.Inactivate();
        items[itemIcon.index] = null;
        panels[itemIcon.index].SetItemNum(0);
        return true;
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

    public ItemIcon[] Where(Func<ItemIcon, bool> func)
    {
        var icons = new List<ItemIcon>();

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            var icon = GetItem(i);
            if (func(icon)) icons.Add(icon);
        }

        return icons.ToArray();
    }

    public DataStoreAgent.ItemInfo[] ExportAllItemInfo()
        => items.Select(icon => icon == null ? null : new DataStoreAgent.ItemInfo(icon.itemInfo.type, icon.itemInfo.numOfItem)).ToArray();
}
