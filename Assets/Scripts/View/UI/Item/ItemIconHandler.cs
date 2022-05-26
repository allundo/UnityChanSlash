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

    protected ItemIndexHandler itemIndex;
    protected ItemSelector selector;
    protected IDisposable cancelSelect = null;

    public IObservable<ItemIcon> OnPutItem => dragMode.onPutItem.DistinctUntilChanged();
    public IObservable<ItemIcon> OnPutApply => putMode.onPutApply;
    public IObservable<ItemInfo> OnUseItem => selectMode.onUseItem;
    public bool IsPutItem => currentMode is PutMode;

    private TweenUtil t = new TweenUtil();
    public Tween Play(Tween tween) => t.PlayExclusive(tween);

    public ItemIconHandler(ItemSelector selector, ItemIndexHandler itemIndex)
    {
        this.selector = selector;
        this.itemIndex = itemIndex;

        pressedIndex = itemIndex.MAX_ITEMS;

        currentMode = normalMode = new NormalMode(this);
        selectMode = new SelectMode(this);
        dragMode = new DragMode(this);
        putMode = new PutMode(this);
    }

    public IItemIconHandler OnPress(int index)
    {
        currentMode = currentMode.OnPress(index);
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

    protected class NormalMode : IItemIconHandler
    {
        protected ItemIconHandler handler;
        protected ItemSelector selector;
        protected ItemIndexHandler itemIndex;

        public NormalMode(ItemIconHandler handler)
        {
            this.handler = handler;
            selector = handler.selector;
            itemIndex = handler.itemIndex;
        }

        public virtual IItemIconHandler OnPress(int index)
        {
            var currentTarget = itemIndex.GetItem(index);

            if (currentTarget != null && currentTarget != handler.currentSelected)
            {
                selector.Enable();
                selector.SetSelect(itemIndex.UIPos(index));
                handler.Play(currentTarget.Resize(1.5f, 0.2f));
                handler.pressedIndex = index;
            }

            return this;
        }

        public virtual IItemIconHandler OnRelease()
        {
            handler.currentSelected = itemIndex.GetItem(handler.pressedIndex);

            if (handler.currentSelected == null) return this;

            handler.cancelSelect?.Dispose();

            // Disable selector when selected item is empty.
            handler.cancelSelect = handler.currentSelected.OnItemEmpty.Subscribe(_ => selector.Disable()).AddTo(selector);

            selector.SetRaycast(true);
            return handler.selectMode;
        }

        public virtual IItemIconHandler OnSubmit() => null;

        public virtual IItemIconHandler CleanUp()
        {
            handler.Play(handler.currentSelected?.Resize(1f, 0.2f));
            handler.currentSelected = null;
            handler.pressedIndex = itemIndex.MAX_ITEMS;

            selector.SetRaycast(false);
            selector.Disable();
            handler.cancelSelect?.Dispose();

            return handler.normalMode;
        }

        public virtual IItemIconHandler OnDrag(Vector2 screenPos) => handler.dragMode.OnDrag(screenPos);
    }

    protected class SelectMode : NormalMode
    {
        public ISubject<ItemInfo> onUseItem { get; protected set; } = new Subject<ItemInfo>();

        public SelectMode(ItemIconHandler handler) : base(handler) { }

        public override IItemIconHandler OnPress(int index)
        {
            var currentTarget = itemIndex.GetItem(index);

            if (currentTarget != null && currentTarget != handler.currentSelected)
            {
                selector.SetRaycast(false);
                selector.SetSelect(itemIndex.UIPos(index));

                handler.currentSelected.Resize(1f, 0.2f).Play();
                handler.currentSelected = null;
                handler.cancelSelect?.Dispose();

                handler.Play(currentTarget.Resize(1.5f, 0.2f));
                handler.pressedIndex = index;

                return handler.normalMode;
            }

            return this;
        }

        public override IItemIconHandler OnRelease() => this;

        public override IItemIconHandler OnSubmit()
        {
            ItemIcon pressed = itemIndex.GetItem(handler.pressedIndex);

            if (pressed == null) return CleanUp();

            if (pressed.itemInfo.attr != ItemAttr.Equipment)
            {
                onUseItem.OnNext(pressed.itemInfo);
            }

            // Continue Select mode when the item is remaining after use it.
            if (itemIndex.GetItem(handler.pressedIndex) != null)
            {
                handler.Play(handler.currentSelected.Resize(0.5f, 0.1f).SetLoops(2, LoopType.Yoyo));
                return this;
            }

            return CleanUp();
        }
    }

    protected class DragMode : NormalMode
    {
        public ISubject<ItemIcon> onPutItem { get; protected set; } = new Subject<ItemIcon>();

        public DragMode(ItemIconHandler handler) : base(handler) { }

        public override IItemIconHandler OnPress(int index)
        {
            selector.SetTarget(itemIndex.UIPos(index));
            handler.pressedIndex = index;
            return this;
        }

        public override IItemIconHandler OnRelease() => this;

        public override IItemIconHandler OnSubmit()
        {
            var pressedIndex = handler.pressedIndex;
            var currentSelected = handler.currentSelected;

            var currentTarget = itemIndex.GetItem(pressedIndex);

            if (currentTarget != null && currentTarget != currentSelected)
            {
                currentTarget.Move(itemIndex.UIPos(currentSelected.index)).Play();
                currentTarget.SetIndex(currentSelected.index);
            }

            itemIndex.SetItem(currentSelected.index, currentTarget);
            handler.Play(currentSelected.Move(itemIndex.UIPos(pressedIndex)));
            currentSelected.SetIndex(pressedIndex);
            itemIndex.SetItem(pressedIndex, currentSelected);

            return base.CleanUp();
        }

        public override IItemIconHandler CleanUp()
        {
            onPutItem.OnNext(null);
            var currentSelected = handler.currentSelected;
            handler.Play(currentSelected.Move(itemIndex.UIPos(currentSelected.index)).SetDelay(0.1f));
            return BaseCleanUp();
        }

        protected IItemIconHandler BaseCleanUp() => base.CleanUp();

        public override IItemIconHandler OnDrag(Vector2 screenPos)
        {
            var vec = itemIndex.ConvertToVec(screenPos);
            return itemIndex.IsOnUI(vec) ? Drag(vec) : Put();
        }

        protected virtual IItemIconHandler Drag(Vector2 uiPos)
        {
            handler.currentSelected.Display(true);
            handler.currentSelected.SetPos(uiPos);
            onPutItem.OnNext(null);
            return handler.dragMode;
        }

        protected IItemIconHandler Put()
        {
            onPutItem.OnNext(handler.currentSelected);
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
            onPutApply.OnNext(handler.currentSelected);
            return CleanUp();
        }

        public override IItemIconHandler CleanUp()
        {
            var currentSelected = handler.currentSelected;

            // Reserve moving back to item inventory
            currentSelected.ResetSize();
            currentSelected.SetParent(selector.transform.parent, true);
            handler.Play(currentSelected.Move(itemIndex.UIPos(currentSelected.index)));

            return BaseCleanUp();
        }

        protected override IItemIconHandler Drag(Vector2 uiPos)
        {
            handler.currentSelected.Display(false);
            onPutItem.OnNext(null);
            return handler.dragMode;
        }
    }
}
