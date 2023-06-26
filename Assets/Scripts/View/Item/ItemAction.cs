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
        var react = target.react as PlayerReactor;
        if (react.HealRatio(1f))
        {
            react.IncPotion();
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
        Pos forward = target.map.GetForward;
        ITile tile = target.map.GetTile(forward);
        if ((tile is Door && (tile as Door).Unlock(type)))
        {
            GameManager.Instance.worldMap.dirMapHandler.Unlock(forward);
            return 1;
        }
        return 0;
    }
}

public class CoinAction : ItemAction
{
    public CoinAction(ItemAttr attr) : base(attr) { }

    public override int Action(PlayerCommandTarget target)
    {
        target.interrupt.OnNext(Command.Data(new PlayerCoinThrow(target, 60f)));
        return 1;
    }
}

public class MagicRingAction : ItemAction
{
    private MagicType bulletType;
    public MagicRingAction(ItemAttr attr, MagicType bulletType) : base(attr)
    {
        this.bulletType = bulletType;
    }

    public override int Action(PlayerCommandTarget target)
    {
        // Force command input while PlayerInput.isCommandValid is false.
        target.interrupt.OnNext(Command.Data(new PlayerFire(target, 40f, bulletType)));
        return 1;
    }
}
