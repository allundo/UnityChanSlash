using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public interface IItemIconHandler
{
    IItemIconHandler OnPress(int index);
    IItemIconHandler OnRelease();
    IItemIconHandler OnSubmit();
    IItemIconHandler OnDrag(Vector2 screenPos);
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

    public IObservable<ItemIcon> OnPutItem => dragMode.onPutItem.DistinctUntilChanged();
    public IObservable<ItemIcon> OnPutApply => putMode.onPutApply;
    public bool IsPutItem => currentMode is PutMode;

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
                currentTarget.Resize(1.5f, 0.2f).Play();
                handler.pressedIndex = index;
            }

            return this;
        }

        public virtual IItemIconHandler OnRelease()
        {
            handler.currentSelected = itemIndex.GetItem(handler.pressedIndex);

            if (handler.currentSelected == null) return this;

            selector.SetRaycast(true);
            return handler.selectMode;
        }

        public virtual IItemIconHandler OnSubmit() => null;

        protected IItemIconHandler CleanUp()
        {
            handler.currentSelected.Resize(1f, 0.2f).Play();
            handler.currentSelected = null;
            handler.pressedIndex = itemIndex.MAX_ITEMS;

            selector.SetRaycast(false);
            selector.Disable();

            return handler.normalMode;
        }

        public virtual IItemIconHandler OnDrag(Vector2 screenPos) => handler.dragMode.OnDrag(screenPos);
    }

    protected class SelectMode : NormalMode
    {
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

                currentTarget.Resize(1.5f, 0.2f).Play();
                handler.pressedIndex = index;

                return handler.normalMode;
            }

            return this;
        }

        public override IItemIconHandler OnRelease() => this;

        public override IItemIconHandler OnSubmit()
        {
            itemIndex.UseItem(handler.pressedIndex);

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
            var currentTarget = itemIndex.GetItem(handler.pressedIndex);
            var currentSelected = handler.currentSelected;

            if (currentTarget != null && currentTarget != currentSelected)
            {
                currentTarget.Move(itemIndex.UIPos(currentSelected.index)).Play();
                currentTarget.SetIndex(currentSelected.index);
            }

            itemIndex.SetItem(currentSelected.index, currentTarget);
            currentSelected.Move(itemIndex.UIPos(pressedIndex)).Play();
            currentSelected.SetIndex(pressedIndex);
            itemIndex.SetItem(pressedIndex, currentSelected);

            return CleanUp();
        }

        public override IItemIconHandler OnDrag(Vector2 screenPos)
        {
            var vec = itemIndex.ConvertToVec(screenPos);
            return itemIndex.IsOnUI(vec) ? Drag(vec) : Put();
        }

        protected IItemIconHandler Drag(Vector2 uiPos)
        {
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
            var currentSelected = handler.currentSelected;

            // Reserve moving back to item inventory
            currentSelected.ResetSize();
            currentSelected.SetParent(selector.transform.parent, true);
            currentSelected.Move(itemIndex.UIPos(currentSelected.index)).Play();

            // Apply put action if possible
            onPutApply.OnNext(currentSelected);
            return CleanUp();
        }
    }
}
