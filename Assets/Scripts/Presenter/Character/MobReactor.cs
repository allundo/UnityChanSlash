using UnityEngine;
using UniRx;
using DG.Tweening;

public interface IReactor
{
    Vector3 position { get; }
    float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None);
    void OnHealRatio(float healRatio = 0f, bool isEffectOn = true);
    void OnFall();
    void OnWakeUp();
    void OnDie();
    void OnMelt(bool isBroken = false);
    void OnActive();
    void FadeOutToDead(float duration = 0.5f);
    void Destroy();
}

public interface IUndeadReactor : IReactor
{
    void OnResurrection();
    void OnSleep();
}

[RequireComponent(typeof(BodyEffect))]
[RequireComponent(typeof(MobInput))]
[RequireComponent(typeof(MapUtil))]
public class MobReactor : MonoBehaviour, IReactor
{
    protected IStatus status;
    protected IMapUtil map;
    protected IBodyEffect effect;
    protected IInput input;
    protected Collider bodyCollider;
    protected Tween fadeOut;

    public Vector3 position => transform.position;

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

        status.Active.Subscribe(_ => OnActive()).AddTo(this);
    }

    protected virtual void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();
    }

    public virtual float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        float damage = CalcDamage(attack, dir, attr);

        if (attr == AttackAttr.Ice)
        {
            if (!status.isIced)
            {
                effect.OnDamage(Mathf.Min(0.01f, damage), type, attr);
                input.InputIced(damage * 100f);
                effect.OnIced(status.corePos);
                status.SetIsIced(true);
            }
            return 0f;
        }
        else if (status.isIced)
        {
            OnMelt(true);
        }

        status.Damage(damage);

        effect.OnDamage(LifeRatio(damage), type, attr);

        return damage;
    }

    public virtual void OnHealRatio(float healRatio = 0f, bool isEffectOn = true)
    {
        if (!status.IsAlive) return;

        if (isEffectOn) effect.OnHeal(healRatio);

        status.Heal(healRatio * status.LifeMax.Value);
    }

    protected float LifeRatio(float life) => Mathf.Clamp01(life / status.LifeMax.Value);

    protected virtual float CalcDamage(float attack, IDirection dir, AttackAttr attr)
    {
        return status.CalcAttack(attack, dir, attr);
    }

    public virtual void OnFall()
    {
        bodyCollider.enabled = false;
    }

    public virtual void OnWakeUp()
    {
        bodyCollider.enabled = true;
    }

    public virtual void OnDie()
    {
        effect.OnDie();
        map.ResetTile();
        bodyCollider.enabled = false;
    }

    public virtual void OnMelt(bool isBroken = false)
    {
        if (!status.isIced) return;

        effect.OnMelt();
        if (isBroken) effect.OnIceCrash(status.corePos);
        status.SetIsIced(false);
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
