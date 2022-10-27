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
    protected EquipmentSource selectedEquipment;
    protected ResourceLoader resourceLoader;
    protected void SelectEquipment()
    {
        selectedEquipment = resourceLoader.GetEquipmentSource(currentSelected.itemInfo.type);
    }

    protected ItemIndexHandler itemIndex;
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

    public ItemIconHandler(ItemSelector selector, ItemIndexHandler itemIndex, EquipItemsHandler equipItems)
    {
        this.selector = selector;
        this.itemIndex = itemIndex;
        this.equipItems = equipItems;
        pressedIndex = itemIndex.MAX_ITEMS;

        pressedInventory = selectedInventory = null;
        selectedEquipment = null;

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
        protected EquipmentSource selectedEquipment
        {
            get { return handler.selectedEquipment; }
            set { handler.selectedEquipment = value; }
        }
        protected IDisposable cancelSelect
        {
            get { return handler.cancelSelect; }
            set { handler.cancelSelect = value; }
        }

        protected ItemIndexHandler itemIndex;
        protected EquipItemsHandler equipItems;

        public NormalMode(ItemIconHandler handler)
        {
            this.handler = handler;
            selector = handler.selector;
            itemIndex = handler.itemIndex;
            equipItems = handler.equipItems;
        }

        public IItemIconHandler OnPress(int index)
        {
            pressedInventory = itemIndex;
            return OnPressInventory(index);
        }

        public virtual IItemIconHandler OnPressEquipment(int index)
        {
            pressedInventory = equipItems;
            return OnPressInventory(index);
        }

        protected virtual IItemIconHandler OnPressInventory(int index)
        {
            handler.StopPressing();

            var currentTarget = pressedInventory.GetItem(index);

            if (currentTarget == null) return this;

            handler.StartLongPressing();

            if (currentTarget != currentSelected)
            {
                selector.Enable();
                selector.SetSelect(pressedInventory.UIPos(index));
                handler.PlaySize(currentTarget.Resize(1.5f, 0.2f));
                pressedIndex = index;
                pressedInventory.ExpandNum(index);
            }

            return this;
        }

        public virtual IItemIconHandler OnRelease()
        {
            handler.StopPressing();

            currentSelected = pressedInventory.GetItem(pressedIndex);

            if (currentSelected == null) return this;

            handler.SelectEquipment();
            selectedInventory = pressedInventory;

            cancelSelect?.Dispose();

            // Disable selector when selected item is empty.
            cancelSelect = currentSelected.OnItemEmpty.Subscribe(_ => selector.Disable()).AddTo(selector);

            selector.SetRaycast(true);
            return handler.selectMode;
        }

        public virtual IItemIconHandler OnSubmit() => null;

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
            pressedIndex = itemIndex.MAX_ITEMS;
            itemIndex.ExpandNum(itemIndex.MAX_ITEMS);
            equipItems.ExpandNum(itemIndex.MAX_ITEMS);

            pressedInventory = selectedInventory = null;
            selectedEquipment = null;

            selector.SetRaycast(false);
            selector.Disable();
            cancelSelect?.Dispose();

            return handler.normalMode;
        }

        public virtual IItemIconHandler OnDrag(Vector2 screenPos)
        {
            handler.StopPressing();
            selectedInventory.DeleteNum(pressedIndex);
            currentSelected.transform.SetAsLastSibling();
            return handler.dragMode.OnDrag(screenPos);
        }
    }

    protected class SelectMode : NormalMode
    {
        public ISubject<ItemInfo> onUseItem { get; protected set; } = new Subject<ItemInfo>();

        public SelectMode(ItemIconHandler handler) : base(handler) { }

        protected override IItemIconHandler OnPressInventory(int index)
        {
            handler.StopPressing();

            var currentTarget = pressedInventory.GetItem(index);

            if (currentTarget == null) return this;

            if (currentTarget != currentSelected)
            {
                handler.StartLongPressing();

                selector.SetRaycast(false);
                selector.SetSelect(pressedInventory.UIPos(index));

                currentSelected.Resize(1f, 0.2f).Play();
                currentSelected = null;
                cancelSelect?.Dispose();

                handler.PlaySize(currentTarget.Resize(1.5f, 0.2f));
                pressedIndex = index;
                pressedInventory.ExpandNum(index);

                return handler.normalMode;
            }

            return this;
        }

        public override IItemIconHandler OnRelease()
        {
            handler.StopPressing();
            return this;
        }

        public override IItemIconHandler OnSubmit()
        {
            ItemIcon pressed = selectedInventory.GetItem(pressedIndex);

            if (pressed == null) return CleanUp();

            if (pressed.itemInfo.attr != ItemAttr.Equipment)
            {
                onUseItem.OnNext(pressed.itemInfo);
                selectedInventory.UpdateItemNum(pressed);
            }

            // Continue Select mode when the item is remaining after use it.
            if (selectedInventory.GetItem(pressedIndex) != null)
            {
                handler.PlaySize(currentSelected.Resize(0.5f, 0.1f).SetLoops(2, LoopType.Yoyo));
                return this;
            }

            return CleanUp();
        }
    }

    protected class DragMode : NormalMode
    {
        public ISubject<ItemIcon> onPutItem { get; protected set; } = new Subject<ItemIcon>();
        private bool IsValidEquipment(int index)
        {
            if (selectedEquipment == null) return false;

            if (index == 1) return selectedEquipment.category == EquipmentCategory.Amulet; // Equip body
            return selectedEquipment.category != EquipmentCategory.Amulet;                 // Equip R or L
        }

        public DragMode(ItemIconHandler handler) : base(handler) { }

        public override IItemIconHandler OnPressEquipment(int index)
        {
            // Don't set target to equip panel if dragging item isn't equipment.
            return IsValidEquipment(index) ? base.OnPressEquipment(index) : this;
        }

        protected override IItemIconHandler OnPressInventory(int index)
        {
            selector.SetTarget(pressedInventory.UIPos(index));
            pressedIndex = index;
            return this;
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

            selectedInventory.SetItem(currentSelected.index, currentTarget, true);
            pressedInventory.SetItem(pressedIndex, currentSelected, true);
            pressedInventory.ExpandNum(pressedIndex);

            if (pressedInventory is ItemIndexHandler)
            {
                selector.Enable();
                selector.SetSelect(pressedInventory.UIPos(pressedIndex));
                selectedInventory = pressedInventory;
                return handler.selectMode;
            }

            return BaseCleanUp();
        }

        public override IItemIconHandler CleanUp()
        {
            onPutItem.OnNext(null);
            currentSelected.MoveExclusive(selectedInventory.UIPos(currentSelected.index), 0.5f, 0.1f);
            return BaseCleanUp();
        }

        protected IItemIconHandler BaseCleanUp() => base.CleanUp();

        public override IItemIconHandler OnDrag(Vector2 screenPos)
        {
            handler.StopPressing();

            var vec = itemIndex.ConvertToVec(screenPos);
            return itemIndex.IsOnUI(vec) || equipItems.IsOnUI(equipItems.ConvertToVec(screenPos)) ? Drag(vec) : Put();
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
