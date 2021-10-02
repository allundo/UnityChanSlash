using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(MobEffect))]
[RequireComponent(typeof(MobCommander))]
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
    protected MobEffect effect;
    protected MobCommander commander;
    protected MobInput input;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        effect = GetComponent<MobEffect>();
        commander = GetComponent<MobCommander>();
        input = GetComponent<MobInput>();
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

        status.OnActive
            .Subscribe(duration => ActiveFadeIn(duration));

        commander.OnDead
            .Subscribe(_ => FadeOutToDead())
            .AddTo(this);
    }

    protected void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
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
        float damageRatio = Mathf.Clamp(damage / status.LifeMax.Value, 0.0f, 1.0f);

        effect.OnDamage(damageRatio);
        lifeGauge?.OnDamage(damageRatio);

        status.Damage(damage);
    }

    protected virtual float CalcDamage(float attack, IDirection dir)
    {
        return status.CalcAttack(attack, dir);
    }

    protected virtual void OnDie()
    {
        effect.OnDie();
        input.InputDie();
    }

    public virtual void ActiveFadeIn(float duration = 0.5f)
    {
        input.ValidateInput(true);
        effect.FadeInTween(duration).Play();
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
        commander.Inactivate();
    }
}
