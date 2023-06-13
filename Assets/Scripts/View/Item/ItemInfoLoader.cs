using System;
using System.Linq;
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
            retItemInfo[type] = new ItemInfo(itemData.Param((int)type), type, itemActions[type], 1);
        }

        return retItemInfo;
    }

    public ItemInfoLoader(ItemData itemData)
    {
        this.itemData = itemData;

        itemActions = new Dictionary<ItemType, ItemAction>()
        {
            { ItemType.Null,            null                                                            },
            { ItemType.Potion,          new PotionAction(ItemAttr.Consumption)                          },
            { ItemType.BrassKnuckle,    new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.BaghNakh,        new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.Jamadhar,        new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.LongSword,       new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.Katana,          new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.KeyBlade,        new KeyItemAction(ItemAttr.Consumption, ItemType.KeyBlade)      },
            { ItemType.Buckler,         new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.CrossShield,     new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.LargeShield,     new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.BattleShield,    new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.CherryNecklace,  new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.RubyNecklace,    new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.SnowNecklace,    new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.CrossNecklace,   new ItemAction(ItemAttr.Equipment)                              },
            { ItemType.Coin,            new CoinAction(ItemAttr.Consumption)                            },
            { ItemType.FireRing,        new MagicRingAction(ItemAttr.Ring, MagicType.FireBall)         },
            { ItemType.IceRing,         new MagicRingAction(ItemAttr.Ring, MagicType.IceBullet)        },
            { ItemType.DarkRing,        new MagicRingAction(ItemAttr.Ring, MagicType.PlayerDarkHound)  },
            { ItemType.TreasureKey,     new KeyItemAction(ItemAttr.Consumption, ItemType.TreasureKey)   },
        };
    }

    public List<ItemType> GetEquipmentList()
     => itemActions
        .Where(act => act.Value == null || act.Value.attr == ItemAttr.Equipment || act.Key == ItemType.KeyBlade)
        .Select(act => act.Key)
        .ToList();
}

