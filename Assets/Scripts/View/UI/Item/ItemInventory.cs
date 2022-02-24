using UnityEngine;
using System.Linq;
using UniRx;
using System;

[RequireComponent(typeof(ItemIconGenerator))]
public class ItemInventory : MonoBehaviour
{
    [SerializeField] private ItemSelector selector = default;
    [SerializeField] private ItemPanel prefabItemPanel = default;

    private ItemIconGenerator iconGenerator;
    private ItemIndexHandler itemIndex;

    private static readonly int WIDTH = 5;
    private static readonly int HEIGHT = 6;
    private static readonly int MAX_ITEMS = WIDTH * HEIGHT;

    private ItemPanel[] panels;

    private ItemIconHandler iconHandler = null;
    public IObservable<ItemIcon> OnPutItem => iconHandler.OnPutItem;
    public IObservable<ItemIcon> OnPutApply => iconHandler.OnPutApply;
    public IObservable<ItemInfo> OnUseItem => iconHandler.OnUseItem;
    public bool IsPutItem => iconHandler.IsPutItem;

    void Awake()
    {
        iconGenerator = GetComponent<ItemIconGenerator>();
        iconGenerator.Init(transform);

        itemIndex = new ItemIndexHandler(GetComponent<RectTransform>(), WIDTH, HEIGHT);

        panels = Enumerable
            .Range(0, MAX_ITEMS)
            .Select(
                index => Instantiate(prefabItemPanel, transform, false)
                    .SetPos(itemIndex.UIPos(index))
                    .SetIndex(index)
            )
            .ToArray();

        selector.transform.SetAsLastSibling(); // Bring selector UI to front

        iconHandler = new ItemIconHandler(selector, itemIndex);
    }

    void Start()
    {
        Observable.Merge(panels.Select(panel => panel.OnPress)).Subscribe(index => iconHandler.OnPress(index));
        Observable.Merge(panels.Select(panel => panel.OnRelease)).Subscribe(index => iconHandler.OnRelease());
        selector.OnDragMode.Subscribe(dragPos => iconHandler.OnDrag(dragPos));
        selector.OnReleased.Subscribe(_ => iconHandler.OnSubmit());
    }

    public bool PickUp(ItemInfo itemInfo)
    {
        for (int index = 0; index < MAX_ITEMS; index++)
        {
            if (SetItem(index, itemInfo)) return true;
        }

        return false;
    }

    public bool Remove(ItemIcon itemIcon) => itemIndex.RemoveItem(itemIcon);

    private bool SetItem(int index, ItemInfo itemInfo)
    {
        if (itemIndex.GetItem(index) != null) return false;

        itemIndex.SetItem(index, iconGenerator.Spawn(itemIndex.UIPos(index), itemInfo).SetIndex(index));
        return true;
    }

    public void SetEnable(bool isEnable)
    {
        if (!isEnable) iconHandler.CleanUp();
        selector.SetEnable(isEnable);
        panels.ForEach(panel => panel.enabled = isEnable);
        enabled = isEnable;
    }
}
