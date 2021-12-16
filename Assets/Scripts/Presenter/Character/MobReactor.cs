using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(IBodyEffect))]
[RequireComponent(typeof(MobInput))]
public class MobReactor : SpawnObject<MobReactor>
{
    /// <summary>
    /// Reaction to life gauge is only supported for player for now.<br />
    /// Leave it empty for the other characters.<br />
    /// TODO: Create LifeGauge interface to apply general reaction.
    /// </summary>
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;

    protected MobStatus status;
    protected IBodyEffect effect;
    protected MobInput input;
    protected Collider bodyCollider;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        effect = GetComponent<IBodyEffect>();
        input = GetComponent<MobInput>();
        bodyCollider = GetComponentInChildren<Collider>();
    }

    protected virtual void Start()
    {
        status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        status.LifeMax
            .SkipLatestValueOnSubscribe()
            .Subscribe(lifeMax => OnLifeMaxChange(lifeMax))
            .AddTo(this);

        lifeGauge?.UpdateLifeText(status.Life.Value, status.LifeMax.Value);
    }

    protected virtual void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();

        lifeGauge?.OnLifeChange(life, status.LifeMax.Value);
    }

    protected void OnLifeMaxChange(float lifeMax)
    {
        lifeGauge?.OnLifeChange(status.Life.Value, lifeMax);
    }

    public virtual void OnDamage(float attack, IDirection dir, AttackType type = AttackType.None)
    {
        if (!status.IsAlive) return;

        float damage = CalcDamage(attack, dir);
        float damageRatio = LifeRatio(damage);

        effect.OnDamage(damageRatio, type);
        lifeGauge?.OnDamage(damageRatio);

        status.Damage(damage);
    }

    public virtual void OnHealRatio(float healRatio = 0f, bool isEffectOn = true)
    {
        if (!status.IsAlive) return;

        float heal = healRatio * status.LifeMax.Value;
        float lifeRatio = LifeRatio(status.Life.Value + heal);

        if (isEffectOn)
        {
            effect.OnHeal(healRatio);
            lifeGauge?.OnHeal(healRatio, lifeRatio);
        }
        else if (status.Life.Value < status.LifeMax.Value)
        {
            lifeGauge?.OnNoEffectHeal(heal, status.Life.Value);
            if (lifeRatio == 1f) lifeGauge?.OnLifeMax();
        }

        status.Heal(heal);
    }

    private float LifeRatio(float life) => Mathf.Clamp01(life / status.LifeMax.Value);

    protected virtual float CalcDamage(float attack, IDirection dir)
    {
        return status.CalcAttack(attack, dir);
    }

    public virtual void OnDie()
    {
        effect.OnDie();
        bodyCollider.enabled = false;
    }

    public override MobReactor OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        status.SetPosition(pos, dir);
        Activate();
        return this;
    }

    public MobReactor SetAttack(float attack)
    {
        status.Attack = attack;
        return this;
    }

    public virtual void FadeOutOnDead(float duration = 0.5f)
    {
        effect.FadeOutTween(duration)
            .OnComplete(Dead)
            .Play();
    }

    protected virtual void Dead() => Inactivate();

    public override void Activate()
    {
        status.OnActive();
        effect.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    public override void Inactivate()
    {
        status.OnInactivate();
    }
}
