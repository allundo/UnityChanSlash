using UnityEngine;
using DG.Tweening;

public class ItemIcon : UISymbol
{
    public ItemInfo itemInfo { get; private set; }

    private UITween ui;

    private Vector2 HandleIconPos => new Vector2(-parentPos.x, -parentPos.y - 320f);
    private Vector2 defaultSize;

    public int index { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ui = new UITween(gameObject);
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
        Move(pos, duration).Play();
    }

    public ItemIcon CopyInfo(Item item)
        => SetItemInfo(item.itemInfo).SetMaterial(item.material);

    public ItemIcon SetItemInfo(ItemInfo info)
    {
        this.itemInfo = info;
        return this;
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

    public ItemIcon SetMaterial(Material material)
    {
        image.material = material;
        return this;
    }

    public ItemIcon SetIndex(int index)
    {
        this.index = index;
        return this;
    }

    public bool UseItem()
    {
        // TODO: Item effect

        bool isEmpty = itemInfo.UseItem() == 0;

        if (isEmpty) Inactivate();

        return isEmpty;
    }

    public Vector2 GetPos() => rectTransform.anchoredPosition;

    public void SetParent(Transform parent, bool worldPositionStays = true, bool setAsFirstSibling = false)
    {
        transform.SetParent(parent, worldPositionStays);
        if (setAsFirstSibling) transform.SetAsFirstSibling();
    }

}
