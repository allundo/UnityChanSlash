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
            retItemInfo[type] = new ItemInfo(itemData.Param((int)type), type, itemActions[type]);
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
            { ItemType.KeyBlade,    new KeyBladeAction(ItemAttr.Consumption, ItemType.KeyBlade) },
            { ItemType.Coin,        new CoinAction(ItemAttr.Consumption)                        },
            { ItemType.FireRing,    new MagicRingAction(ItemAttr.Ring, BulletType.FireBall)     },
            { ItemType.IceRing,     new MagicRingAction(ItemAttr.Ring, BulletType.IceBullet)    },
            { ItemType.DarkRing,    new MagicRingAction(ItemAttr.Ring, BulletType.DarkHound)    },
        };
    }
}

