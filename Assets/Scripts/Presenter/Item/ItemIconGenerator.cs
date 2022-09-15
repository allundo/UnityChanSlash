using UnityEngine;

public class ItemIconGenerator : UISymbolGenerator
{
    public ItemIcon Spawn(Vector2 pos, ItemInfo itemInfo = null)
    {
        var itemIcon = base.Spawn(pos) as ItemIcon;

        if (itemInfo == null) return itemIcon;

        return itemIcon.CopyInfo(itemInfo);
    }
    public ItemIcon Respawn(Vector2 pos, ItemType type, int numOfItem)
        => Spawn(pos, ResourceLoader.Instance.ItemInfo(type, numOfItem)).CancelAnim();
}
