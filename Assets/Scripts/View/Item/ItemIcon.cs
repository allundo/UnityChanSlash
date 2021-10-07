using UnityEngine;
using DG.Tweening;

public class ItemIcon : UISymbol
{
    private ItemInfo itemInfo;

    public override UISymbol OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5F)
    {
        SetPos(pos, duration);
        Activate();
        return this;
    }

    public ItemIcon SetPos(Vector3 pos, float duration = 0.5f)
    {

        // Set localPosition to -parentPos is equivalent to set position to screen center
        rectTransform.localPosition = new Vector2(-parentPos.x, -parentPos.y - 300f);

        // Moving animation from front Tile to item inventory
        rectTransform.DOAnchorPos(pos, duration).SetEase(Ease.OutExpo).Play();
        return this;
    }

    public ItemIcon CopyInfo(Item item)
        => SetItemInfo(item.itemInfo).SetMaterial(item.material);

    public ItemIcon SetItemInfo(ItemInfo info)
    {
        this.itemInfo = info;
        return this;
    }

    public ItemIcon SetMaterial(Material material)
    {
        image.material = material;
        return this;
    }
}
