using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : Reactor
{
    protected override void Awake()
    {
        base.Awake();
        effect = GetComponent<BulletEffect>();

    }
    public override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
    }

    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        status.Damage(10f);
        effect.OnDamage(1f, type, attr);

        return 10f;
    }

    public override void OnDie()
    {
        effect.OnDie();
        effect.Disappear(OnDead);
    }
}
