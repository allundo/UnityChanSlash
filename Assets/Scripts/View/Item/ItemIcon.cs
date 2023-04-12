using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class ItemIcon : UISymbol
{
    public ItemInfo itemInfo { get; private set; }
    protected Vector2 parentPos;

    private Vector2 HandleIconPos => new Vector2(-parentPos.x, -parentPos.y - 320f);
    private Vector2 defaultSize;

    public int index { get; private set; }
    public bool isEquip { get; private set; }

    public IObservable<int> OnNumOfItemItemChange => itemInfo.OnNumOfItemChange;

    public bool TryMerge(ItemInfo itemInfo)
    {
        if (this.itemInfo.attr != ItemAttr.Equipment && this.itemInfo.type == itemInfo.type)
        {
            this.itemInfo.Merge(itemInfo);
            return true;
        }

        return false;
    }

    private Tween moveTween;
    public ItemIcon CancelAnim()
    {
        moveTween?.Complete(true);
        return this;
    }

    protected override void Awake()
    {
        base.Awake();
        parentPos = transform.parent.GetComponent<RectTransform>().localPosition;
        defaultSize = rectTransform.sizeDelta;
    }

    public override UISymbol OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5F)
    {
        SetToInventory(pos, duration);
        Activate();
        return this;
    }

    private void SetToInventory(Vector3 pos, float duration = 0.5f)
    {
        // Set localPosition to -parentPos is equivalent to set position to screen center
        rectTransform.localPosition = HandleIconPos;

        // Moving animation from front Tile to item inventory
        MoveExclusive(pos, duration);
    }

    public ItemIcon CopyInfo(ItemInfo itemInfo)
        => SetItemInfo(itemInfo).SetMaterial();

    public ItemIcon SetItemInfo(ItemInfo info)
    {
        this.itemInfo = info;
        return this;
    }

    public Tween MoveExclusive(Vector2 destPos, float duration = 0.5f, float delay = 0f)
    {
        moveTween?.Complete(true);
        moveTween = Move(destPos, duration).SetDelay(delay).Play();
        return moveTween;
    }

    public Tween Move(Vector2 destPos, float duration = 0.5f)
    {
        return rectTransform.DOAnchorPos(destPos, duration).SetEase(Ease.OutExpo);
    }
    public Tween LocalMove(Vector2 destPos, float duration = 0.5f)
    {
        return rectTransform.DOLocalMove(destPos, duration).SetEase(Ease.OutExpo);
    }

    public Tween MoveToHandleIcon(float duration = 0.5f) => LocalMove(HandleIconPos, duration);

    public Tween Resize(float ratio = 1f, float duration = 0.5f)
    {
        return rectTransform.DOSizeDelta(defaultSize * ratio, duration);
    }
    public void ResetSize(float ratio = 1f)
    {
        rectTransform.sizeDelta = defaultSize * ratio;
    }

    public ItemIcon SetMaterial()
    {
        image.material = itemInfo.material;
        return this;
    }

    public ItemIcon SetIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ItemIcon SetInventoryType(bool isEquip)
    {
        this.isEquip = isEquip;
        return this;
    }

    public void SetParent(Transform parent, bool worldPositionStays = true, bool setAsFirstSibling = false)
    {
        transform.SetParent(parent, worldPositionStays);
        if (setAsFirstSibling) transform.SetAsFirstSibling();
    }

    public void Display(bool isShow = true) => image.material.color = new Color(1, 1, 1, Convert.ToInt32(isShow));
}
