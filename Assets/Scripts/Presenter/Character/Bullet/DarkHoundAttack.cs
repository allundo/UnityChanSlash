using UnityEngine;

public class DarkHoundAttack : BulletAttack
{
    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        var damage = targetMob.OnDamage(status.attack * attackMultiplier, status.dir, attackType, attackAttr);
        reactor.OnDamage(damage, null, AttackType.None, AttackAttr.None);
        return targetMob;
    }
}
