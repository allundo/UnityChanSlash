using UnityEngine;

public interface IMobReactor : IReactor
{
    void OnHealRatio(float healRatio = 0f, bool isEffectOn = true);
    void OnFall();
    void OnWakeUp();
    void OnMelt(bool isBroken = false);
    void OnDisappear(float duration = 0.5f);
    void OnHide();
    bool OnAppear();
}

public interface IUndeadReactor : IMobReactor
{
    void OnResurrection();
    void OnSleep();
}

[RequireComponent(typeof(MobEffect))]
public class MobReactor : Reactor, IMobReactor
{
    protected IMobEffect mobEffect;

    protected override void Awake()
    {
        base.Awake();
        effect = mobEffect = GetComponent<MobEffect>();
    }

    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive || status.isHidden) return 0f;

        float damage = CalcDamage(attack, dir, attr);

        if (attr == AttackAttr.Ice)
        {
            if (!status.isIced)
            {
                effect.OnDamage(Mathf.Min(0.01f, damage), type, attr);
                input.InputIced(damage * 100f);
                mobEffect.OnIced(status.corePos);
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

        if (isEffectOn) mobEffect.OnHeal(healRatio);

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

    public virtual void OnMelt(bool isBroken = false)
    {
        if (!status.isIced) return;

        mobEffect.OnMelt();
        if (isBroken) mobEffect.OnIceCrash(status.corePos);
        status.SetIsIced(false);
    }

    public virtual void OnHide()
    {
        if (status.isHidden) return;

        status.SetHidden(true);
        mobEffect.OnHide();
    }

    public virtual bool OnAppear()
    {
        if (!status.isHidden) return true;

        status.SetHidden(false);
        mobEffect.OnAppear();
        return true;
    }

    public virtual void OnDisappear(float duration = 0.5f)
    {
        mobEffect.Disappear(OnDead, duration);
    }
}
