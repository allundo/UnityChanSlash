using UnityEngine;
public class PotionInfo : ItemInfo
{
    public PotionInfo(Material material, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null)
        : base(material, numOfItem, vfx, sfx) { }

    protected override void OnAction(MobReactor react, MobAnimator anim)
    {
        react.OnHealRatio(1f);
    }

    public override object Clone(int numOfItem)
        => new PotionInfo(material, numOfItem, vfx, sfx);
}