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

    private ItemIconHandler iconHandler = null;
    public IObservable<ItemIcon> OnPutItem => iconHandler.OnPutItem;
    public IObservable<ItemIcon> OnPutApply => iconHandler.OnPutApply;
    public IObservable<ItemInfo> OnUseItem => iconHandler.OnUseItem;
    public IObservable<MessageData[]> OnInspectItem => iconHandler.OnInspectItem.Select(itemInfo => MessageData.ItemDescription(itemInfo));
    public bool IsPutItem => iconHandler.IsPutItem;

    void Awake()
    {
        iconGenerator = GetComponent<ItemIconGenerator>();
        iconGenerator.Init(transform);

        itemIndex = new ItemIndexHandler(GetComponent<RectTransform>(), WIDTH, HEIGHT).SetPanels(prefabItemPanel);

        selector.transform.SetAsLastSibling(); // Bring selector UI to front

        iconHandler = new ItemIconHandler(selector, itemIndex);
    }

    void Start()
    {
        itemIndex.OnPress.Subscribe(index => iconHandler.OnPress(index)).AddTo(this);
        itemIndex.OnRelease.Subscribe(index => iconHandler.OnRelease()).AddTo(this);

        selector.OnLongPress.Subscribe(_ => iconHandler.OnLongPress()).AddTo(this);
        selector.OnDragMode.Subscribe(dragPos => iconHandler.OnDrag(dragPos)).AddTo(this);
        selector.OnReleased.Subscribe(_ => iconHandler.OnSubmit()).AddTo(this);
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
        itemIndex.SetEnablePanels(isEnable);
        enabled = selector.enabled = isEnable;
    }

    public void Cancel() => iconHandler.CleanUp();

    public bool hasKeyBlade()
    {
        for (int index = 0; index < MAX_ITEMS; index++)
        {
            var itemIcon = itemIndex.GetItem(index);
            if (itemIcon == null) continue;
            if (itemIcon.itemInfo.type == ItemType.KeyBlade) return true;
        }
        return false;
    }

    public ulong SumUpPrices()
    {
        ulong amount = 0;
        itemIndex
            .Select(itemIcon => itemIcon != null ? itemIcon.itemInfo.Price : 0)
            .ForEach(price => amount += (ulong)price);
        return amount;
    }
}
