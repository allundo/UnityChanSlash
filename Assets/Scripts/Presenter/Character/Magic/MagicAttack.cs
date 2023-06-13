using UnityEngine;

public class MagicAttack : Attack
{
    protected MagicReactor reactor;
    protected AttackData data;

    protected override void Awake()
    {
        base.Awake();
        data = new AttackData(attackMultiplier, attackType, attackAttr);
        reactor = GetComponentInParent<MagicReactor>();
    }

    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        var bulletStatus = status as MagicStatus;

        targetMob.Damage(Shooter.New(bulletStatus.Attack, bulletStatus.shotBy), data);
        reactor.Damage(status.LifeMax.Value, null, AttackType.None, AttackAttr.None);
        return targetMob;
    }
}
