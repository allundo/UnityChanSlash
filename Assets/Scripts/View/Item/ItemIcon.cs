using UnityEngine;

public class ItemIcon : UISymbol
{
    private ItemInfo itemInfo;

    public override UISymbol OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5F)
    {
        SetPos(pos);
        Activate();
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
