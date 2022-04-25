using UnityEngine;

public class PotionInfo : ItemInfo
{
    public PotionInfo(ItemSource itemSource, int numOfItem = 1)
        : base(itemSource, numOfItem) { }

    public PotionInfo(Material material, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null)
        : base(material, numOfItem, vfx, sfx) { }

    protected override int Action(PlayerCommandTarget target)
    {
        return (target.react as IMobReactor).HealRatio(1f) ? 1 : 0;
    }

    public override object Clone(int numOfItem)
        => new PotionInfo(material, numOfItem, vfx, sfx);
}