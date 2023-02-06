using UnityEngine;
using System.Linq;
using UniRx;
using System;
using TMPro;

[RequireComponent(typeof(ItemIconGenerator))]
public class ItemInventory : SingletonMonoBehaviour<ItemInventory>
{
    [SerializeField] private ItemSelector selector = default;
    [SerializeField] private ItemPanel prefabItemPanel = default;
    [SerializeField] private ItemPanel prefabEquipPanel = default;
    [SerializeField] private RectTransform rtEquipItems = default;
    [SerializeField] private TextMeshProUGUI tmRightHandNone = default;
    [SerializeField] private TextMeshProUGUI tmLeftHandNone = default;
    [SerializeField] private TextMeshProUGUI tmBodyAccessoryNone = default;

    private ItemIconGenerator iconGenerator;
    protected InventoryItemsHandler inventoryItems;
    protected EquipItemsHandler equipItems;
    public Vector2 uiOrigin => inventoryItems.uiOrigin;

    private static readonly int WIDTH = 5;
    private static readonly int HEIGHT = 4;
    private static readonly int MAX_ITEMS = WIDTH * HEIGHT;

    private ItemIconHandler iconHandler = null;
    public IObservable<ItemIcon> OnPutItem => iconHandler.OnPutItem;
    public IObservable<ItemIcon> OnPutApply => iconHandler.OnPutApply;
    public IObservable<ItemInfo> OnUseItem => iconHandler.OnUseItem;
    public IObservable<MessageData[]> OnInspectItem => iconHandler.OnInspectItem.Select(itemInfo => MessageData.ItemDescription(itemInfo));

    public IObservable<ItemInfo> EquipR => equipItems.EquipR;
    public IObservable<ItemInfo> EquipL => equipItems.EquipL;
    public IObservable<ItemInfo> EquipBody => equipItems.EquipBody;

    public IObservable<IEquipmentStyle> FightStyleChange => equipItems.CurrentEquipments;

    public bool IsPutItem => iconHandler.IsPutItem;

    public bool UseEquip(int index) => iconHandler.UseEquip(index);
    public void UseShield() => iconHandler.UseShield();

    protected override void Awake()
    {
        base.Awake();

        iconGenerator = GetComponent<ItemIconGenerator>();

        inventoryItems = new InventoryItemsHandler(this, prefabItemPanel, WIDTH, HEIGHT);
        equipItems = new EquipItemsHandler(this, rtEquipItems, prefabEquipPanel);

        selector.transform.SetAsLastSibling(); // Bring selector UI to front

        iconHandler = new ItemIconHandler(selector, inventoryItems, equipItems);
    }

    void Start()
    {
        inventoryItems.OnPress.Subscribe(index => iconHandler.OnPress(index)).AddTo(this);
        equipItems.OnPress.Subscribe(index => iconHandler.OnPressEquipment(index)).AddTo(this);
        Observable.Merge(inventoryItems.OnRelease, equipItems.OnRelease)
            .Subscribe(index => iconHandler.OnRelease()).AddTo(this);

        Observable.Merge(inventoryItems.SetBack, equipItems.SetBack)
            .Where(pair => pair.targetPlace != null)
            .Subscribe(pair => (pair.targetPlace.isEquip ? equipItems as IItemIndexHandler : inventoryItems).SetItem(pair.targetPlace.index, pair.itemToSet, true))
            .AddTo(this);

        selector.OnLongPress.Subscribe(_ => iconHandler.OnLongPress()).AddTo(this);
        selector.OnDragMode.Subscribe(dragPos => iconHandler.OnDrag(dragPos)).AddTo(this);
        selector.OnReleased.Subscribe(_ => iconHandler.OnSubmit()).AddTo(this);

        EquipR.Select(item => item == null)
            .Subscribe(isEquipNull => tmRightHandNone.enabled = isEquipNull)
            .AddTo(this);

        EquipL.Select(item => item == null)
            .Subscribe(isEquipNull => tmLeftHandNone.enabled = isEquipNull)
            .AddTo(this);

        EquipBody.Select(item => item == null)
            .Subscribe(isEquipNull => tmBodyAccessoryNone.enabled = isEquipNull)
            .AddTo(this);
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        inventoryItems.ResetOrientation(orientation);
        equipItems.ResetOrientation(orientation);
    }

    public bool PickUp(ItemInfo itemInfo)
    {
        for (int index = 0; index < MAX_ITEMS; index++)
        {
            if (SetItem(index, itemInfo)) return true;
        }

        return false;
    }

    public bool Remove(ItemIcon itemIcon)
    {
        IItemIndexHandler handler = itemIcon.isEquip ? equipItems : inventoryItems;
        return handler.RemoveItem(itemIcon);
    }

    private bool SetItem(int index, ItemInfo itemInfo)
    {
        if (inventoryItems.GetItem(index) != null) return false;

        inventoryItems.SetItem(index, iconGenerator.Spawn(inventoryItems.UIPos(index), itemInfo));
        return true;
    }

    public void SetEnable(bool isEnable)
    {
        inventoryItems.SetEnablePanels(isEnable);
        equipItems.SetEnablePanels(isEnable);
        enabled = selector.enabled = isEnable;
        if (!isEnable) Cancel();
    }

    public void SetEquipEnable(bool isEnable)
    {
        equipItems.SetEnablePanels(isEnable);
        if (!isEnable && selector.isEquip) Cancel();
    }

    public void Cancel() => iconHandler.CleanUp();

    public bool hasKeyBlade()
    {
        return inventoryItems.hasKeyBlade() || equipItems.hasKeyBlade();
    }

    public ulong SumUpPrices()
    {
        return inventoryItems.SumUpPrices() + equipItems.SumUpPrices();
    }

    public DataStoreAgent.ItemInfo[] ExportInventoryItems()
    {
        var itemList = inventoryItems.ExportAllItemInfo().ToList();
        itemList.AddRange(equipItems.ExportAllItemInfo());
        return itemList.ToArray();
    }

    public void ImportInventoryItems(DataStoreAgent.ItemInfo[] import)
    {
        for (int i = 0; i < import.Length; i++)
        {
            var itemInfo = import[i];

            // Imported class data cannot be null but filled by default field values.
            if (itemInfo == null || itemInfo.itemType == ItemType.Null) continue;

            IItemIndexHandler handler = i < MAX_ITEMS ? inventoryItems : equipItems;
            int index = i % MAX_ITEMS;

            handler.SetItem(index, iconGenerator.Respawn(handler.UIPos(index), itemInfo.itemType, itemInfo.numOfItem));
        }
    }

    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
}
