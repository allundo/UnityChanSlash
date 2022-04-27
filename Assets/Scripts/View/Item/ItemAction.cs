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
        return (tile is Door && (tile as Door).UnLock(type)) ? 1 : 0;
    }
}
