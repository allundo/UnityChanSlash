using UnityEngine;

public class BulletAttack : Attack
{
    protected BulletReactor reactor;

    protected override void Awake()
    {
        base.Awake();
        reactor = GetComponentInParent<BulletReactor>();
    }

    protected override void OnHitAttack(Collider collider)
    {
        MobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack * attackMultiplier, status.dir);
        reactor.OnDamage(status.LifeMax.Value, null);
    }
}
