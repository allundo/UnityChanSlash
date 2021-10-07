using UnityEngine;

public class ItemIconGenerator : Generator<UISymbol>
{
    protected override void Awake()
    {
        spawnPoint = Vector3.zero;
    }

    public void Init(Transform parentTransform)
    {
        pool = parentTransform;
    }

    public ItemIcon Spawn(Vector2 pos, Item item = null)
    {
        var itemIcon = base.Spawn(pos) as ItemIcon;

        if (item == null) return itemIcon;

        return itemIcon.CopyInfo(item);
    }
}