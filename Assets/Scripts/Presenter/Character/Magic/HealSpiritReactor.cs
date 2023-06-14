using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MagicStatus))]
[RequireComponent(typeof(SpiritEffect))]
public class HealSpiritReactor : MortalReactor
{
    protected MagicStatus bulletStatus;
    protected IMagicEffect effect;
    protected bool isTrailing = false;
    protected Tween emittingTween = null;

    protected override void Awake()
    {
        base.Awake();
        bulletStatus = status as MagicStatus;
        effect = GetComponent<SpiritEffect>();
    }

    protected virtual void Update()
    {
        if (!isTrailing) return;
        transform.position += (bulletStatus.shotBy.Position - transform.position).normalized * Time.deltaTime * 2.5f;
        ReduceHP(Time.deltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!status.IsAlive) return;

        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (bulletStatus.shotBy.gameObject != targetMob?.gameObject) return;

        AffectTarget(targetMob);

        effect.OnHit();
        Die();
    }

    protected virtual void AffectTarget(IMobReactor target)
    {
        target.Heal(status.attack);
    }

    public override void OnDie()
    {
        bodyCollider.enabled = false;
        isTrailing = false;
        effect.Disappear(OnDead, 1f);
    }

    protected override void OnActive()
    {
        effect.OnActive();
        bodyCollider.enabled = isTrailing = false;
        emittingTween = transform.DOMove(UnityEngine.Random.onUnitSphere * 2f, 0.5f)
            .SetRelative(true)
            .OnComplete(() => bodyCollider.enabled = isTrailing = true)
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
