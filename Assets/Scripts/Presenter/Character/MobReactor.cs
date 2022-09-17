using UnityEngine;

public interface IMobReactor : IReactor
{
    /// <summary>
    /// Apply healing by rate of life max.
    /// </summary>
    /// <param name="healRatio"></param>
    /// <param name="isEffectOn">Applies healing effect if true.</param>
    /// <param name="healAnyway">Applies the healing anyway even when it isn't effective if true.</param>
    /// <returns>true if the healing is applied</returns>
    bool HealRatio(float healRatio = 0f, bool isEffectOn = true, bool healAnyway = false);
    bool Heal(float life, bool isEffectOn = true, bool healAnyway = false);
    void OnFall();
    void OnWakeUp();
    void Iced(float icingFrames);
    void Melt(bool isBroken = false);
    void OnDisappear(float duration = 0.5f);
    void Hide();

    /// <summary>
    /// Returns from hiding.
    /// </summary>
    /// <returns>true if appearing is valid</returns>
    bool Appear();
}

public interface IUndeadReactor : IMobReactor
{
    void OnResurrection();
    void OnSleep();
}

[RequireComponent(typeof(MobEffect))]
[RequireComponent(typeof(MobMapUtil))]
[RequireComponent(typeof(MobInput))]
[RequireComponent(typeof(MobStatus))]
public class MobReactor : Reactor, IMobReactor
{
    protected IMobStatus mobStatus;
    protected IMobMapUtil map;
    protected IMobInput input;
    protected IMobEffect effect;
    protected MobAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        effect = GetComponent<MobEffect>();
        mobStatus = status as IMobStatus;
        map = GetComponent<MobMapUtil>(); ;
        input = GetComponent<MobInput>();
        anim = GetComponent<MobAnimator>();
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();
    }

    public override float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!mobStatus.IsAlive || mobStatus.isHidden) return 0f;

        float damage = CalcDamage(attack, dir, attr);

        if (attr == AttackAttr.Ice)
        {
            if (damage > 0 && mobStatus.icingFrames == 0f)
            {
                var icingFrames = damage * 100f;
                input.InputIced(icingFrames);
                effect.OnDamage(Mathf.Min(0.01f, damage), type, attr);
            }
            return 0f;
        }
        else if (mobStatus.icingFrames > 0f)
        {
            Melt(true);
        }

        mobStatus.LifeChange(-damage);

        effect.OnDamage(LifeRatio(damage), type, attr);

        return damage;
    }

    public virtual bool HealRatio(float healRatio = 0f, bool isEffectOn = true, bool healAnyway = false)
    {
        if (!mobStatus.IsAlive || (mobStatus.IsLifeMax && !healAnyway)) return false;

        if (isEffectOn && mobStatus.icingFrames == 0f && healRatio > 0.1f) effect.OnHeal(healRatio);

        mobStatus.LifeChange(healRatio * status.LifeMax.Value);

        return true;
    }

    public bool Heal(float life, bool isEffectOn = true, bool healAnyway = false)
        => HealRatio(LifeRatio(life), isEffectOn, healAnyway);

    protected float LifeRatio(float life) => Mathf.Clamp(life / mobStatus.LifeMax.Value, -1f, 1f);

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

    public virtual void Iced(float icingFrames)
    {
        mobStatus.SetIcingFrames(icingFrames);
        effect.OnIced(mobStatus.corePos);
    }

    public virtual void Melt(bool isBroken = false)
    {
        if (mobStatus.icingFrames == 0f) return;

        effect.OnMelt();
        if (isBroken)
        {
            input.OnIceCrash();
            effect.OnIceCrash(mobStatus.corePos);
        }
        mobStatus.SetIcingFrames(0f);
    }

    public virtual void Hide()
    {
        if (mobStatus.isHidden) return;

        mobStatus.SetHidden(true);
        effect.OnHide();
    }

    public virtual bool Appear()
    {
        if (!mobStatus.isHidden) return true;

        mobStatus.SetHidden(false);
        effect.OnAppear();
        return true;
    }

    public virtual void OnDisappear(float duration = 0.5f)
    {
        effect.Disappear(OnDead, duration);
    }

    protected override void OnDead()
    {
        status.Inactivate();
        anim.die.Bool = false;
    }

    public override void OnDie()
    {
        map.ResetTile();
        effect.OnDie();
        bodyCollider.enabled = false;
    }

    protected override void OnActive()
    {
        effect.OnActive();
        map.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    public override void Destroy()
    {
        map.ResetTile();
        input.ClearAll();
        effect.OnDestroyByReactor();
        bodyCollider.enabled = false;
        Destroy(gameObject);
    }
}
