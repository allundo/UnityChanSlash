using UnityEngine;

public class WitchDoubleAttack : MobAttack
{
    protected AttackData data;

    protected override void Awake()
    {
        base.Awake();
        data = new AttackData(attackMultiplier, attackType, attackAttr);
    }

    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        var bulletStatus = status as MagicStatus;

        targetMob.Damage(Shooter.New(bulletStatus.Attack, bulletStatus.shotBy), data);
        return targetMob;
    }
}
