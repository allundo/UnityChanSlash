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
    {
        if ((target.react as IMobReactor).HealRatio(1f))
        {
            return 1;
        }
        else
        {
            ActiveMessageController.Instance.InputMessageData(new ActiveMessageData("飲む必要なし！"));
            return 0;
        }
    }
}

public class KeyItemAction : ItemAction
{
    protected ItemType type;
    public KeyItemAction(ItemAttr attr, ItemType type) : base(attr)
    {
        this.type = type;
    }

    public override int Action(PlayerCommandTarget target)
    {
        ITile tile = target.map.ForwardTile;
        return (tile is Door && (tile as Door).UnLock(type)) ? 1 : 0;
    }
}

public class CoinAction : ItemAction
{
    public CoinAction(ItemAttr attr) : base(attr) { }

    public override int Action(PlayerCommandTarget target)
    {
        target.input.Interrupt(new PlayerCoinThrow(target, 60f), false);
        return 1;
    }
}

public class MagicRingAction : ItemAction
{
    private BulletType bulletType;
    public MagicRingAction(ItemAttr attr, BulletType bulletType) : base(attr)
    {
        this.bulletType = bulletType;
    }

    public override int Action(PlayerCommandTarget target)
    {
        // Force command input while PlayerInput.isCommandValid is false.
        target.input.Interrupt(new PlayerFire(target, 40f, bulletType), false);
        return 1;
    }
}
