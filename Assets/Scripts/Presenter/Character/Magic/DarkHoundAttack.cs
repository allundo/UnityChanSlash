using UnityEngine;

public class DarkHoundAttack : BulletAttack
{
    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        var damage = targetMob.Damage(status.attack * attackMultiplier, Direction.Convert(transform.forward), attackType, attackAttr);
        reactor.Damage(damage, null, AttackType.None, AttackAttr.None);
        return targetMob;
    }
}
