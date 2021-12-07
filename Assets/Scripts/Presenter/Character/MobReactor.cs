using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(IBodyEffect))]
[RequireComponent(typeof(MobInput))]
public class MobReactor : MonoBehaviour
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

        status.OnActive.Subscribe(_ => OnActive()).AddTo(this);
    }

    protected virtual void OnLifeChange(float life)
    {
        if (life <= 0.0f) Die();

        lifeGauge?.OnLifeChange(life, status.LifeMax.Value);
    }

    protected void OnLifeMaxChange(float lifeMax)
    {
        lifeGauge?.OnLifeChange(status.Life.Value, lifeMax);
    }

    public virtual void OnDamage(float attack, IDirection dir)
    {
        if (!status.IsAlive) return;

        float damage = CalcDamage(attack, dir);
        float damageRatio = LifeRatio(damage);

        effect.OnDamage(damageRatio);
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
        else if (lifeRatio == 1f)
        {
            effect.OnLifeMax();
        }

        status.Heal(heal);
    }

    private float LifeRatio(float life) => Mathf.Clamp01(life / status.LifeMax.Value);

    protected virtual float CalcDamage(float attack, IDirection dir)
    {
        return status.CalcAttack(attack, dir);
    }

    protected virtual void OnActive()
    {
        input.ValidateInput(true);
        effect.OnActive();
        bodyCollider.enabled = true;
    }

    protected virtual void Die()
    {
        input.InputDie();
        effect.OnDie();
        bodyCollider.enabled = false;
    }

    public virtual void FadeOutToDead(float duration = 0.5f)
    {
        effect.FadeOutTween(duration)
            .OnComplete(Dead)
            .Play();
    }
    protected virtual void Dead() => Inactivate();

    protected void Inactivate()
    {
        status.Inactivate();
    }
}
