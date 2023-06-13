using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(HealSpritEffect))]
public class HealSpiritReactor : Reactor, IBulletReactor
{
    protected BulletStatus bulletStatus;
    protected IBodyEffect effect;
    protected bool isTweening = false;
    protected Tween emittingTween = null;

    protected override void Awake()
    {
        base.Awake();
        bulletStatus = status as BulletStatus;
        effect = GetComponent<HealSpritEffect>();
    }

    protected virtual void Update()
    {
        if (isTweening) return;
        transform.position += (bulletStatus.shotBy.Position - transform.position).normalized * Time.deltaTime * 2.5f;
        ReduceHP(Time.deltaTime);
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (bulletStatus.shotBy.gameObject != targetMob?.gameObject) return;

        targetMob.Heal(status.attack);
        Damage(20f, null);
        bodyCollider.enabled = false;
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

        return 10f;
    }

    public override void OnDie()
    {
        bodyCollider.enabled = false;
        effect.OnDie();
        effect.Disappear(OnDead);
        isTweening = true;
    }

    protected override void OnActive()
    {
        effect.OnActive();
        bodyCollider.enabled = true;
        isTweening = true;
        emittingTween = transform.DOMove(UnityEngine.Random.onUnitSphere * 2f, 0.5f)
            .SetRelative(true)
            .OnComplete(() => isTweening = false)
            .Play();
    }

    public override void Destroy()
    {
        // Stop all tweens before destroying
        effect.OnDestroyByReactor();
        bodyCollider.enabled = false;

        Destroy(gameObject);
    }
}