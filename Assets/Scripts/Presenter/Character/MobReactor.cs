using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(BodyEffect))]
[RequireComponent(typeof(MobInput))]
[RequireComponent(typeof(MapUtil))]
public class MobReactor : MonoBehaviour
{
    /// <summary>
    /// Reaction to life gauge is only supported for player for now.<br />
    /// Leave it empty for the other characters.<br />
    /// TODO: Create LifeGauge interface to apply general reaction.
    /// </summary>
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;

    protected MobStatus status;
    protected MapUtil map;
    protected IBodyEffect effect;
    protected IInput input;
    protected Collider bodyCollider;
    protected Tween fadeOut;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        effect = GetComponent<BodyEffect>();
        input = GetComponent<MobInput>();
        map = GetComponent<MapUtil>();
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

        status.Active.Subscribe(_ => OnActive()).AddTo(this);

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

    protected float LifeRatio(float life) => Mathf.Clamp01(life / status.LifeMax.Value);

    protected virtual float CalcDamage(float attack, IDirection dir)
    {
        return status.CalcAttack(attack, dir);
    }

    public virtual void OnDie()
    {
        effect.OnDie();
        map.ResetTile();
        bodyCollider.enabled = false;
    }

    public virtual void OnActive()
    {
        effect.OnActive();
        map.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    public virtual void FadeOutToDead(float duration = 0.5f)
    {
        fadeOut = effect.FadeOutTween(duration)
            .OnComplete(OnDead)
            .Play();
    }

    protected virtual void OnDead() => status.Inactivate();

    public virtual void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        fadeOut?.Kill();
        effect.KillAllTweens();

        bodyCollider.enabled = false;
        map.ResetTile();

        Destroy(gameObject);
    }
}
