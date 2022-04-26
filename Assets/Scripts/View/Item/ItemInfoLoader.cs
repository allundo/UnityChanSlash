using System;
using System.Collections.Generic;

public class ItemInfoLoader
{
    protected ItemData itemData;
    protected Dictionary<ItemType, Func<PlayerCommandTarget, int>> itemUseActions;

    public Dictionary<ItemType, ItemInfo> LoadItemInfo()
    {
        var retItemInfo = new Dictionary<ItemType, ItemInfo>();

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            retItemInfo[type] = new ItemInfo(itemData.Param((int)type), itemUseActions[type]);
        }

        return retItemInfo;
    }

    public ItemInfoLoader(ItemData itemData)
    {
        this.itemData = itemData;

        itemUseActions = new Dictionary<ItemType, Func<PlayerCommandTarget, int>>()
        {
            { ItemType.Potion,      target => (target.react as IMobReactor).HealRatio(1f) ? 1 : 0       },
            { ItemType.KeyBlade,    KeyBladeAction                                                      },
        };
    }

    protected int KeyBladeAction(PlayerCommandTarget target)
    {
        ITile tile = target.map.ForwardTile;
        if (tile is Door)
        {
            // TODO: implement unlocking door process
            return 1;
        }

        return 0;
    }
}