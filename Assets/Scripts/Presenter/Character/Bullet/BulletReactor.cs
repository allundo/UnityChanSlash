using UnityEngine;

public interface IBulletReactor : IReactor
{
    void ReduceHP(float reduction = 1f);
}

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : Reactor, IBulletReactor
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

    public void ReduceHP(float reduction = 1f)
    {
        if (status.IsAlive) status.Damage(reduction);
    }

    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        status.Damage(attack);
        effect.OnDamage(attack, type, attr);

        return attack;
    }

    public override void OnDie()
    {
        effect.OnDie();
        effect.Disappear(OnDead);
        map.ResetTile();
    }
}
