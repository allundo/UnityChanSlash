using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class ItemIconHandler
{
    private static ItemIconHandler instance = null;
    protected static NormalMode normalMode = null;
    protected static SelectMode selectMode = null;
    protected static DragMode dragMode = null;
    protected static PutMode putMode = null;
    private static ItemIconHandler currentMode = null;

    protected static int pressedIndex;
    protected static ItemIcon currentSelected = null;

    protected ItemSelector selector;
    protected ItemIndexHandler itemIndex;

    public struct ItemPutInfo
    {
        public bool isPutActive;
        public ItemIcon itemIcon;
    }

    public static IObservable<ItemIcon> OnPutItem => dragMode.onPutItem.DistinctUntilChanged();
    public static IObservable<ItemIcon> OnPutApply => putMode.onPutApply;
    public static bool IsPutItem => currentMode is PutMode;

    private ItemIconHandler(ItemSelector selector, ItemIndexHandler itemIndex)
    {
        this.selector = selector;
        this.itemIndex = itemIndex;
    }

    public static ItemIconHandler New(ItemSelector selector, ItemIndexHandler itemIndex)
    {
        if (instance == null)
        {
            instance = new ItemIconHandler(selector, itemIndex);

            pressedIndex = itemIndex.MAX_ITEMS;

            currentMode = normalMode = new NormalMode(selector, itemIndex);
            selectMode = new SelectMode(selector, itemIndex);
            dragMode = new DragMode(selector, itemIndex);
            putMode = new PutMode(selector, itemIndex);
        }

        return instance;
    }

    public virtual ItemIconHandler OnPress(int index)
    {
        currentMode = currentMode.OnPress(index);
        return currentMode;
    }

    public virtual ItemIconHandler OnRelease(int index)
    {
        currentMode = currentMode.OnRelease(index);
        return currentMode;
    }
    public virtual ItemIconHandler OnSubmit()
    {
        currentMode = currentMode.OnSubmit();
        return currentMode;
    }

    public void OnDrag(Vector2 screenPos)
    {
        var vec = itemIndex.ConvertToVec(screenPos);
        currentMode = itemIndex.IsOnUI(vec) ? currentMode.Drag(vec) : currentMode.Put();
    }

    public virtual ItemIconHandler Drag(Vector2 uiPos) => dragMode.Drag(uiPos);
    public virtual ItemIconHandler Put() => dragMode.Put();

    protected ItemIconHandler CleanUp()
    {
        currentSelected = null;
        pressedIndex = itemIndex.MAX_ITEMS;

        selector.SetRaycast(false);
        selector.Disable();

        return normalMode;
    }

    protected class NormalMode : ItemIconHandler
    {
        public NormalMode(ItemSelector selector, ItemIndexHandler itemIndex) : base(selector, itemIndex)
        { }

        public override ItemIconHandler OnPress(int index)
        {
            var currentTarget = itemIndex.GetItem(index);

            if (currentTarget != null && currentTarget != currentSelected)
            {
                selector.Enable();
                selector.SetSelect(itemIndex.UIPos(index));
                pressedIndex = index;
            }

            return this;
        }

        public override ItemIconHandler OnRelease(int index)
        {
            currentSelected = itemIndex.GetItem(pressedIndex);
            selector.SetRaycast(true);

            return selectMode;
        }

        public override ItemIconHandler OnSubmit() => null;
    }

    protected class SelectMode : NormalMode
    {
        public SelectMode(ItemSelector selector, ItemIndexHandler itemIndex) : base(selector, itemIndex) { }

        public override ItemIconHandler OnPress(int index)
        {
            if (itemIndex.GetItem(index) != currentSelected)
            {
                selector.SetRaycast(false);
                currentSelected = null;

                return normalMode;
            }

            return this;
        }

        public override ItemIconHandler OnSubmit()
        {
            itemIndex.UseItem(pressedIndex);

            return CleanUp();
        }
    }

    protected class DragMode : ItemIconHandler
    {
        public ISubject<ItemIcon> onPutItem { get; protected set; } = new Subject<ItemIcon>();

        public DragMode(ItemSelector selector, ItemIndexHandler itemIndex) : base(selector, itemIndex) { }

        public override ItemIconHandler OnPress(int index)
        {
            selector.SetTarget(itemIndex.UIPos(index));
            pressedIndex = index;
            return this;
        }

        public override ItemIconHandler OnRelease(int index) => this;

        public override ItemIconHandler OnSubmit()
        {
            var currentTarget = itemIndex.GetItem(pressedIndex);

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

        public override ItemIconHandler Drag(Vector2 uiPos)
        {
            currentSelected.SetPos(uiPos);
            onPutItem.OnNext(null);
            return dragMode;
        }

        public override ItemIconHandler Put()
        {
            onPutItem.OnNext(currentSelected);
            return putMode;
        }
    }

    protected class PutMode : DragMode
    {
        public ISubject<ItemIcon> onPutApply { get; protected set; } = new Subject<ItemIcon>();

        public PutMode(ItemSelector selector, ItemIndexHandler itemIndex) : base(selector, itemIndex) { }

        public override ItemIconHandler OnSubmit()
        {
            // Reserve moving back to item inventory
            currentSelected.ResetSize();
            currentSelected.Move(itemIndex.UIPos(currentSelected.index)).Play();
            currentSelected.SetParent(selector.transform.parent, false);

            // Apply put action if possible
            onPutApply.OnNext(currentSelected);
            return CleanUp();
        }
    }
}
