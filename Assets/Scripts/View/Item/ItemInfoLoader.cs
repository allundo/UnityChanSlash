using System;
using System.Collections.Generic;

public class ItemInfoLoader
{
    protected ItemData itemData;
    protected Dictionary<ItemType, ItemAction> itemActions;

    public Dictionary<ItemType, ItemInfo> LoadItemInfo()
    {
        var retItemInfo = new Dictionary<ItemType, ItemInfo>();

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            retItemInfo[type] = new ItemInfo(itemData.Param((int)type), itemActions[type]);
        }

        return retItemInfo;
    }

    public ItemInfoLoader(ItemData itemData)
    {
        this.itemData = itemData;

        itemActions = new Dictionary<ItemType, ItemAction>()
        {
            { ItemType.Null,        null                                                        },
            { ItemType.Potion,      new PotionAction(ItemAttr.Consumption)                      },
            { ItemType.KeyBlade,    new KeyBladeAction(ItemAttr.Equipment, ItemType.KeyBlade)   },
        };
    }
}

public class ItemAction
{
    public ItemAttr attr { get; protected set; }
    public ItemAction(ItemAttr attr)
    {
        this.attr = attr;
    }
    /// <summary>
    /// Item effect.
    /// </summary>
    /// <param name="target"></param>
    /// <returns>The number of item consumption.</returns>
    public virtual int Action(PlayerCommandTarget target) => 0;
}

public class PotionAction : ItemAction
{
    public PotionAction(ItemAttr attr) : base(attr) { }
    public override int Action(PlayerCommandTarget target)
         => (target.react as IMobReactor).HealRatio(1f) ? 1 : 0;
}

public class KeyBladeAction : ItemAction
{
    protected ItemType type;
    public KeyBladeAction(ItemAttr attr, ItemType type) : base(attr)
    {
        this.type = type;
    }

    public override int Action(PlayerCommandTarget target)
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