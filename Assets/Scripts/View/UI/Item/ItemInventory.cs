using UnityEngine;
using System.Linq;
using UniRx;
using System;

[RequireComponent(typeof(ItemIconGenerator))]
public class ItemInventory : MonoBehaviour
{
    [SerializeField] private ItemSelector selector = default;
    [SerializeField] private ItemPanel prefabItemPanel = default;
    [SerializeField] private ItemPanel prefabEquipPanel = default;
    [SerializeField] private RectTransform rtEquipItems = default;

    private ItemIconGenerator iconGenerator;
    protected ItemIndexHandler itemIndex;
    protected EquipItemsHandler equipItems;

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

        itemIndex = new ItemIndexHandler(GetComponent<RectTransform>(), WIDTH, HEIGHT).SetPanels(prefabItemPanel);
        equipItems = new EquipItemsHandler(rtEquipItems, itemIndex.inventoryOrigin).SetPanels(prefabEquipPanel);

        selector.transform.SetAsLastSibling(); // Bring selector UI to front

        iconHandler = new ItemIconHandler(selector, itemIndex, equipItems);
    }

    void Start()
    {
        itemIndex.OnPress.Subscribe(index => iconHandler.OnPress(index)).AddTo(this);
        equipItems.OnPress.Subscribe(index => iconHandler.OnPressEquipment(index)).AddTo(this);
        Observable.Merge(itemIndex.OnRelease, equipItems.OnRelease)
            .Subscribe(index => iconHandler.OnRelease()).AddTo(this);

        selector.OnLongPress.Subscribe(_ => iconHandler.OnLongPress()).AddTo(this);
        selector.OnDragMode.Subscribe(dragPos => iconHandler.OnDrag(dragPos)).AddTo(this);
        selector.OnReleased.Subscribe(_ => iconHandler.OnSubmit()).AddTo(this);
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        itemIndex.ResetOrientation(orientation);
        itemIndex.UpdateOrigin();
        equipItems.UpdateOrigin();
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
            .Where(itemIcon => itemIcon != null)
            .ForEach(itemIcon => amount += (ulong)itemIcon.itemInfo.Price);
        return amount;
    }

    public DataStoreAgent.ItemInfo[] ExportInventoryItems() => itemIndex.ExportAllItemInfo();
    public void ImportInventoryItems(DataStoreAgent.ItemInfo[] import)
    {
        for (int index = 0; index < import.Length; index++)
        {
            var itemInfo = import[index];

            // Imported class data cannot be null but filled by default field values.
            if (itemInfo == null || itemInfo.itemType == ItemType.Null) continue;

            itemIndex.SetItem(index, iconGenerator.Respawn(itemIndex.UIPos(index), itemInfo.itemType, itemInfo.numOfItem).SetIndex(index));
        }
    }
}
