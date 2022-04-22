using UnityEngine;

public class BulletAttack : Attack
{
    protected BulletReactor reactor;

    protected override void Awake()
    {
        base.Awake();
        reactor = GetComponentInParent<BulletReactor>();
    }

    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        targetMob.Damage(status.attack * attackMultiplier, status.dir, attackType, attackAttr);
        reactor.Damage(status.LifeMax.Value, null, AttackType.None, AttackAttr.None);
        return targetMob;
    }
}
