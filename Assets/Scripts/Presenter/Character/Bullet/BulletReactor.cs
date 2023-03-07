using UnityEngine;

public interface IBulletReactor : IReactor
{
    void ReduceHP(float reduction = 1f);
    float CurrentHP { get; }
}

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
[RequireComponent(typeof(MapUtil))]
public class BulletReactor : Reactor, IBulletReactor
{
    protected IMapUtil map;
    protected IInput input;
    protected IBodyEffect effect;

    protected override void Awake()
    {
        base.Awake();
        effect = GetComponent<BulletEffect>();
        input = GetComponent<BulletInput>();
        map = GetComponent<MapUtil>();
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InterruptDie();
    }

    protected override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
    }

    public float CurrentHP => status.Life.Value;

    public void ReduceHP(float reduction = 1f)
    {
        if (status.IsAlive) status.LifeChange(-reduction);
    }


    public override float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        status.LifeChange(-attack);
        effect.OnDamage(attack, type, attr);

        return attack;
    }

    public override void OnDie()
    {
        effect.OnDie();
        effect.Disappear(OnDead);
    }

    public override void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        effect.OnDestroyByReactor();

        bodyCollider.enabled = false;

        Destroy(gameObject);
    }
}
