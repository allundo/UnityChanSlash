using UnityEngine;

public class ItemIconGenerator : MobGenerator<UISymbol>
{
    protected override void Awake()
    {
        spawnPoint = Vector3.zero;
    }

    public void Init(Transform parentTransform)
    {
        pool = parentTransform;
    }

    public ItemIcon Spawn(Vector2 pos, ItemInfo itemInfo = null)
    {
        var itemIcon = base.Spawn(pos) as ItemIcon;

        if (itemInfo == null) return itemIcon;

        return itemIcon.CopyInfo(itemInfo);
    }
}
