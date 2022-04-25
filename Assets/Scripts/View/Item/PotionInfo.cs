using UnityEngine;
public class PotionInfo : ItemInfo
{
    public PotionInfo(ItemSource itemSource, int numOfItem = 1)
        : base(itemSource, numOfItem) { }

    public PotionInfo(Material material, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null)
        : base(material, numOfItem, vfx, sfx) { }

    protected override void Action(PlayerCommandTarget target)
    {
        (target.react as IMobReactor).HealRatio(1f);
    }

    public override object Clone(int numOfItem)
        => new PotionInfo(material, numOfItem, vfx, sfx);
}