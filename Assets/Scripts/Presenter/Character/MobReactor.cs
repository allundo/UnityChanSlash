using UnityEngine;

public interface IMobReactor : IReactor
{
    void HealRatio(float healRatio = 0f, bool isEffectOn = true);
    void Heal(float life, bool isEffectOn = true);
    void OnFall();
    void OnWakeUp();
    void Melt(bool isBroken = false);
    void OnDisappear(float duration = 0.5f);
    void Hide();
    bool Appear();
}

public interface IUndeadReactor : IMobReactor
{
    void OnResurrection();
    void OnSleep();
}

[RequireComponent(typeof(MobEffect))]
[RequireComponent(typeof(MobMapUtil))]
public class MobReactor : Reactor, IMobReactor
{
    protected IMobStatus mobStatus;
    protected IMobMapUtil mobMap;
    protected IMobInput mobInput;
    protected IMobEffect mobEffect;

    protected override void Awake()
    {
        base.Awake();
        effect = mobEffect = GetComponent<MobEffect>();
        mobStatus = status as IMobStatus;
        mobMap = map as IMobMapUtil;
        mobInput = input as IMobInput;
    }

    public override float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!mobStatus.IsAlive || mobStatus.isHidden) return 0f;

        float damage = CalcDamage(attack, dir, attr);

        if (attr == AttackAttr.Ice)
        {
            if (damage > 0 && !mobStatus.isIced)
            {
                effect.OnDamage(Mathf.Min(0.01f, damage), type, attr);
                mobInput.InputIced(damage * 100f);
                mobEffect.OnIced(mobStatus.corePos);
                mobStatus.SetIsIced(true);
            }
            return 0f;
        }
        else if (mobStatus.isIced)
        {
            Melt(true);
        }

        mobStatus.LifeChange(-damage);

        effect.OnDamage(LifeRatio(damage), type, attr);

        return damage;
    }

    public virtual void HealRatio(float healRatio = 0f, bool isEffectOn = true)
    {
        if (!mobStatus.IsAlive) return;

        if (isEffectOn && !mobStatus.isIced && healRatio > 0.1f) mobEffect.OnHeal(healRatio);

        mobStatus.LifeChange(healRatio * status.LifeMax.Value);
    }

    public void Heal(float life, bool isEffectOn = true)
        => HealRatio(LifeRatio(life), isEffectOn);

    protected float LifeRatio(float life) => Mathf.Clamp01(life / mobStatus.LifeMax.Value);

    protected virtual float CalcDamage(float attack, IDirection dir, AttackAttr attr)
    {
        return mobStatus.CalcAttack(attack, dir, attr);
    }

    public virtual void OnFall()
    {
        bodyCollider.enabled = false;
    }

    public virtual void OnWakeUp()
    {
        bodyCollider.enabled = true;
    }

    public virtual void Melt(bool isBroken = false)
    {
        if (!mobStatus.isIced) return;

        mobEffect.OnMelt();
        if (isBroken) mobEffect.OnIceCrash(mobStatus.corePos);
        mobStatus.SetIsIced(false);
    }

    public virtual void Hide()
    {
        if (mobStatus.isHidden) return;

        mobStatus.SetHidden(true);
        mobEffect.OnHide();
    }

    public virtual bool Appear()
    {
        if (!mobStatus.isHidden) return true;

        mobStatus.SetHidden(false);
        mobEffect.OnAppear();
        return true;
    }

    public virtual void OnDisappear(float duration = 0.5f)
    {
        mobEffect.Disappear(OnDead, duration);
    }

    public override void OnDie()
    {
        mobMap.ResetTile();
        base.OnDie();
    }

    public override void Destroy()
    {
        mobMap.ResetTile();
        base.Destroy();
    }
}
