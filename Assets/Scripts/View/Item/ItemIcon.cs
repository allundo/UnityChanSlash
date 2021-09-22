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
        rectTransform.localPosition = new Vector2(-parentPos.x, -parentPos.y - 300f);
        rectTransform.DOAnchorPos(pos, duration).SetEase(Ease.OutExpo).Play();
        return this;
    }

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
