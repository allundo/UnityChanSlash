using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public interface IItemIconHandler
{
    /// <summary>
    /// OnPointerEnter into a panel.
    /// </summary>
    /// <param name="index">panel index</param>
    /// <returns>next state icon handler</returns>
    IItemIconHandler OnPress(int index);

    /// <summary>
    /// OnPointerEnter into a equipment panel.
    /// </summary>
    /// <param name="index">panel index</param>
    /// <returns>next state icon handler</returns>
    IItemIconHandler OnPressEquipment(int index);

    /// <summary>
    /// OnPointerExit from pressing panel.
    /// </summary>
    /// <returns>next state icon handler</returns>
    IItemIconHandler OnRelease();

    /// <summary>
    /// OnPointerUp from selected panel.
    /// </summary>
    /// <returns>next state icon handler</returns>
    IItemIconHandler OnSubmit();

    /// <summary>
    /// OnDrag an item icon.
    /// </summary>
    /// <returns>next state icon handler</returns>
    IItemIconHandler OnDrag(Vector2 screenPos);

    /// <summary>
    /// Back to the normal mde icon handler mode and neutral state.
    /// </summary>
    /// <returns>normal mode icon handler</returns>
    IItemIconHandler CleanUp();

    /// <summary>
    /// Returns long pressed item info.
    /// </summary>
    /// <returns>ItemInfo to be inspected</returns>
    IItemIconHandler OnLongPress();
}

public class ItemIconHandler : IItemIconHandler
{
    protected NormalMode normalMode = null;
    protected SelectMode selectMode = null;
    protected DragMode dragMode = null;
    protected PutMode putMode = null;
    private IItemIconHandler currentMode = null;

    protected int pressedIndex;
    protected ItemIcon currentSelected = null;

    protected IItemIndexHandler pressedInventory;
    protected IItemIndexHandler selectedInventory;
    protected ResourceLoader resourceLoader;

    protected EquipmentSource GetEquipmentSource(ItemIcon item)
    {
        return resourceLoader.GetEquipmentSource(item.itemInfo.type);
    }

    protected InventoryItemsHandler inventoryItems;
    protected EquipItemsHandler equipItems;
    protected ItemSelector selector;
    protected IDisposable cancelSelect = null;

    public IObservable<ItemIcon> OnPutItem => dragMode.onPutItem.DistinctUntilChanged();
    public IObservable<ItemIcon> OnPutApply => putMode.onPutApply;
    public IObservable<ItemInfo> OnUseItem => selectMode.onUseItem;
    public IObservable<ItemInfo> OnInspectItem => onInspectItem;
    private ISubject<ItemInfo> onInspectItem = new Subject<ItemInfo>();

    private IDisposable longPress;

    protected void StartLongPressing(int dueTimeFrameCount = 60)
    {
        longPress = Observable.TimerFrame(dueTimeFrameCount).Subscribe(_ => OnLongPress()).AddTo(selector);
    }

    protected void StopPressing()
    {
        longPress?.Dispose();
    }

    public bool IsPutItem => currentMode is PutMode;
    private TweenUtil sizeUtil = new TweenUtil();
    private TweenUtil moveUtil = new TweenUtil();
    public Tween PlaySize(Tween sizeTween) => sizeUtil.PlayExclusive(sizeTween);
    public Tween PlayMove(Tween moveTween) => moveUtil.PlayExclusive(moveTween);

    public ItemIconHandler(ItemSelector selector, InventoryItemsHandler inventoryItems, EquipItemsHandler equipItems)
    {
        this.selector = selector;
        this.inventoryItems = inventoryItems;
        this.equipItems = equipItems;
        pressedIndex = inventoryItems.MAX_ITEMS;

        pressedInventory = selectedInventory = null;

        currentMode = normalMode = new NormalMode(this);
        selectMode = new SelectMode(this);
        dragMode = new DragMode(this);
        putMode = new PutMode(this);

        resourceLoader = ResourceLoader.Instance;
    }

    public IItemIconHandler OnPress(int index)
    {
        currentMode = currentMode.OnPress(index);
        return currentMode;
    }

    public IItemIconHandler OnPressEquipment(int index)
    {
        currentMode = currentMode.OnPressEquipment(index);
        return currentMode;
    }

    public IItemIconHandler OnRelease()
    {
        currentMode = currentMode.OnRelease();
        return currentMode;
    }
    public IItemIconHandler OnSubmit()
    {
        currentMode = currentMode.OnSubmit();
        return currentMode;
    }

    public IItemIconHandler OnDrag(Vector2 screenPos)
    {
        currentMode = currentMode.OnDrag(screenPos);
        return currentMode;
    }

    public IItemIconHandler CleanUp()
    {
        currentMode = currentMode.CleanUp();
        return currentMode;
    }

    public IItemIconHandler OnLongPress()
    {
        currentMode = currentMode.OnLongPress();
        return currentMode;
    }

    public bool UseEquip(int index)
    {
        bool isEquipValid = equipItems.UseEquip(index);
        if (!isEquipValid)
        {
            CleanUp();
            equipItems.SetItem(index, null);
        }
        return isEquipValid;
    }

    public void UseShield()
    {
        var shield = equipItems.UseShield();
        if (shield != null)
        {
            CleanUp();
            equipItems.SetItem(shield.index, null);
        }
    }

    protected class NormalMode : IItemIconHandler
    {
        protected ItemIconHandler handler;
        protected ItemSelector selector;

        protected int pressedIndex
        {
            get { return handler.pressedIndex; }
            set { handler.pressedIndex = value; }
        }
        protected ItemIcon currentSelected
        {
            get { return handler.currentSelected; }
            set { handler.currentSelected = value; }
        }
        protected IItemIndexHandler pressedInventory
        {
            get { return handler.pressedInventory; }
            set { handler.pressedInventory = value; }
        }
        protected IItemIndexHandler selectedInventory
        {
            get { return handler.selectedInventory; }
            set { handler.selectedInventory = value; }
        }

        protected IDisposable cancelSelect
        {
            get { return handler.cancelSelect; }
            set { handler.cancelSelect = value; }
        }

        protected InventoryItemsHandler inventoryItems;
        protected EquipItemsHandler equipItems;

        public NormalMode(ItemIconHandler handler)
        {
            this.handler = handler;
            selector = handler.selector;
            inventoryItems = handler.inventoryItems;
            equipItems = handler.equipItems;
        }

        public virtual IItemIconHandler OnPress(int index)
        {
            var prev = pressedInventory;
            pressedInventory = inventoryItems;
            return OnPressInventory(index, prev);
        }

        public virtual IItemIconHandler OnPressEquipment(int index)
        {
            var prev = pressedInventory;
            pressedInventory = equipItems;
            return OnPressInventory(index, prev);
        }

        protected virtual IItemIconHandler OnPressInventory(int index, IItemIndexHandler prevInventory)
        {
            handler.StopPressing();

            var currentTarget = pressedInventory.GetItem(index);

            if (currentTarget == null)
            {
                pressedInventory = null;
                return this;
            }

            handler.StartLongPressing();

            if (currentTarget != currentSelected)
            {
                cancelSelect?.Dispose();
                selector.SetRaycast(false);
                selector.SetSelect(pressedInventory.UIPos(index), pressedInventory is EquipItemsHandler);
                handler.PlaySize(currentTarget.Resize(1.5f, 0.2f));
                pressedIndex = index;
                prevInventory?.ShrinkNum();
                pressedInventory.ExpandNum(index);
            }

            return this;
        }

        public virtual IItemIconHandler OnRelease()
        {
            handler.StopPressing();

            currentSelected = pressedInventory?.GetItem(pressedIndex);

            if (currentSelected == null) return this;

            selectedInventory = pressedInventory;

            // Clean up to normal mode when selected item gets to be empty.
            cancelSelect = currentSelected.OnNumOfItemItemChange
                .Where(num => num == 0)
                .Subscribe(_ => handler.CleanUp())
                .AddTo(selector);

            selector.SetRaycast(true);
            return handler.selectMode;
        }

        public virtual IItemIconHandler OnSubmit()
        {
            Debug.LogError("Item is submitted on NormalMode.");
            return this;
        }

        public IItemIconHandler OnLongPress()
        {
            var itemInfo = pressedInventory.GetItem(pressedIndex)?.itemInfo;
            if (itemInfo != null) handler.onInspectItem.OnNext(itemInfo);
            return OnRelease();
        }

        public virtual IItemIconHandler CleanUp()
        {
            handler.StopPressing();

            handler.PlaySize(currentSelected?.Resize(1f, 0.2f));
            currentSelected = null;
            pressedIndex = inventoryItems.MAX_ITEMS;
            inventoryItems.ShrinkNum();
            equipItems.ShrinkNum();

            pressedInventory = selectedInventory = null;

            selector.SetRaycast(false);
            selector.Disable();
            cancelSelect?.Dispose();

            return handler.normalMode;
        }

        public virtual IItemIconHandler OnDrag(Vector2 screenPos)
        {
            Debug.LogError("Item is dragged on NormalMode.");
            return this;
        }

        protected void EquipMessage(ItemIcon equipment)
        {
            if (equipment != null)
            {
                ActiveMessageController.Instance.Equip(equipment.itemInfo.name);
            }
        }
    }

    protected class SelectMode : NormalMode
    {
        public ISubject<ItemInfo> onUseItem { get; protected set; } = new Subject<ItemInfo>();

        public SelectMode(ItemIconHandler handler) : base(handler) { }

        protected override IItemIconHandler OnPressInventory(int index, IItemIndexHandler prevInventory)
        {
            handler.StopPressing();

            var currentTarget = pressedInventory.GetItem(index);

            if (currentTarget == null || currentTarget == currentSelected)
            {
                pressedInventory = currentSelected.isEquip ? equipItems : inventoryItems;
                return this;
            }

            handler.StartLongPressing();

            selector.SetRaycast(false);
            selector.SetSelect(pressedInventory.UIPos(index), pressedInventory is EquipItemsHandler);

            currentSelected.Resize(1f, 0.2f).Play();
            currentSelected = null;
            cancelSelect?.Dispose();

            handler.PlaySize(currentTarget.Resize(1.5f, 0.2f));
            pressedIndex = index;
            prevInventory?.ShrinkNum();
            pressedInventory.ExpandNum(index);

            return handler.normalMode;
        }

        public override IItemIconHandler OnRelease()
        {
            handler.StopPressing();
            selector.SetRaycast(true);
            return this;
        }

        public override IItemIconHandler OnSubmit()
        {
            ItemIcon selected = selectedInventory.GetItem(pressedIndex);

            if (selected == null) return CleanUp();

            bool isEquip = selected.itemInfo.attr == ItemAttr.Equipment;

            // In the case of item use. Also KeyBlade is usable.
            if (!isEquip) onUseItem.OnNext(selected.itemInfo);

            // Back to normal mode if the item is used up to empty.
            if (currentSelected == null) return handler.normalMode;

            // In the case of item equipment from inventory items.
            var selectedEquipment = handler.GetEquipmentSource(currentSelected);
            if (selectedEquipment != null && selectedInventory is InventoryItemsHandler)
            {
                if (equipItems.isEnable)
                {
                    EquipMessage(selected);
                    equipItems.Equip(selectedEquipment.category, selected);
                    return CleanUp();
                }
                else
                {
                    ActiveMessageController.Instance.InputMessageData(new ActiveMessageData("装備を変えてるスキがない！", SDFaceID.ANGRY2, SDEmotionID.IRRITATE));
                }
            }

            // Item use interaction.
            handler.PlaySize(currentSelected.Resize(0.5f, 0.1f).SetLoops(2, LoopType.Yoyo));
            return this;
        }

        public override IItemIconHandler OnDrag(Vector2 screenPos)
        {
            handler.StopPressing();
            selectedInventory.DeleteNum(pressedIndex);
            currentSelected.transform.SetAsLastSibling();
            return handler.dragMode.OnDrag(screenPos);
        }
    }

    protected class DragMode : NormalMode
    {
        public ISubject<ItemIcon> onPutItem { get; protected set; } = new Subject<ItemIcon>();

        private bool IsValidEquipment(int equipIndex, ItemIcon equipItem)
        {
            if (equipItem == null) return true;

            var equipment = handler.GetEquipmentSource(equipItem);

            if (equipment == null) return false;

            if (equipIndex == 1) return equipment.category == EquipmentCategory.Amulet; // Equip body
            return equipment.category != EquipmentCategory.Amulet;                      // Equip R or L
        }

        public DragMode(ItemIconHandler handler) : base(handler) { }

        public override IItemIconHandler OnPress(int index)
        {
            pressedInventory = inventoryItems;

            // Set target anyway when inventory item is selected.
            // When equip item is selected, check if pressed item is valid equipment.
            if (selectedInventory is InventoryItemsHandler || IsValidEquipment(currentSelected.index, pressedInventory.GetItem(index)))
            {
                SetTarget(index);
            }
            return this;
        }

        public override IItemIconHandler OnPressEquipment(int index)
        {
            pressedInventory = equipItems;

            // Don't set target to equip panel if dragging item isn't equipment.
            if (IsValidEquipment(index, currentSelected))
            {
                SetTarget(index);
            }
            return this;
        }

        private void SetTarget(int index)
        {
            selector.SetTarget(pressedInventory.UIPos(index));
            pressedIndex = index;
        }

        public override IItemIconHandler OnRelease()
        {
            handler.StopPressing();
            return this;
        }

        public override IItemIconHandler OnSubmit()
        {
            var currentTarget = pressedInventory.GetItem(pressedIndex);

            if (currentTarget != null && currentTarget != currentSelected && currentSelected.TryMerge(currentTarget.itemInfo))
            {
                // currentTarget icon is inactivated if the merging is succeeded.
                currentTarget = null;
            }

            if (currentSelected.isEquip != pressedInventory is EquipItemsHandler)
            {
                EquipMessage(currentSelected.isEquip ? currentTarget : currentSelected);
            }

            inventoryItems.ShrinkNum();
            equipItems.ShrinkNum();
            pressedInventory.SwitchItem(pressedIndex, currentSelected);

            if (pressedInventory is InventoryItemsHandler)
            {
                pressedInventory.ExpandNum(pressedIndex);

                selector.SetSelect(pressedInventory.UIPos(pressedIndex), false);
                selectedInventory = pressedInventory;

                return handler.selectMode;
                // OnRelease() will be fired by the pointed panel immediately.
            }

            return BaseCleanUp();
            // OnRelease() will be fired by the pointed panel immediately.
        }

        public override IItemIconHandler CleanUp()
        {
            onPutItem.OnNext(null);
            selectedInventory.UpdateItemNum(currentSelected);
            currentSelected.MoveExclusive(selectedInventory.UIPos(currentSelected.index), 0.5f, 0.1f);
            return BaseCleanUp();
        }

        protected IItemIconHandler BaseCleanUp() => base.CleanUp();

        public override IItemIconHandler OnDrag(Vector2 screenPos)
        {
            handler.StopPressing();

            var vec = inventoryItems.ConvertToVec(screenPos);
            return inventoryItems.IsOnUI(vec) || equipItems.IsOnUI(equipItems.ConvertToVec(screenPos)) ? Drag(vec) : Put();
        }

        protected virtual IItemIconHandler Drag(Vector2 uiPos)
        {
            currentSelected.Display(true);
            currentSelected.SetPos(uiPos);
            onPutItem.OnNext(null);
            return handler.dragMode;
        }

        protected IItemIconHandler Put()
        {
            selector.Hide();
            onPutItem.OnNext(currentSelected);
            return handler.putMode;
        }
    }

    protected class PutMode : DragMode
    {
        public ISubject<ItemIcon> onPutApply { get; protected set; } = new Subject<ItemIcon>();

        public PutMode(ItemIconHandler handler) : base(handler) { }

        public override IItemIconHandler OnSubmit()
        {
            // Apply put action if possible
            onPutApply.OnNext(currentSelected);
            return CleanUp();
        }

        public override IItemIconHandler CleanUp()
        {
            // Reserve moving back to item inventory
            currentSelected.ResetSize();
            currentSelected.SetParent(selector.transform.parent, true);
            currentSelected.MoveExclusive(selectedInventory.UIPos(currentSelected.index));
            selectedInventory.UpdateItemNum(currentSelected);

            return BaseCleanUp();
        }

        protected override IItemIconHandler Drag(Vector2 uiPos)
        {
            selector.Show();
            currentSelected.Display(false);
            onPutItem.OnNext(null);
            return handler.dragMode;
        }
    }
}
