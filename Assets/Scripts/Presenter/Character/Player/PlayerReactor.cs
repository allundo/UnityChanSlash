using UnityEngine;
using UniRx;
using static ShieldInput;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerEffect))]
[RequireComponent(typeof(PlayerAnimFX))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerReactor : MobReactor
{
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;
    [SerializeField] protected RestUI restUI = default;

    protected PlayerAnimator anim;
    protected ParticleSystem iceCrashVFX;

    protected GuardState guardState => (input as PlayerInput).guardState;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<PlayerAnimator>();
        iceCrashVFX = Resources.Load<ParticleSystem>("Prefabs/Effect/FX_ICE_CRASH");
    }

    protected override void Start()
    {
        status.Activate();
        anim.lifeRatio.Float = 1f;

        base.Start();

        status.LifeMax
            .SkipLatestValueOnSubscribe()
            .Subscribe(lifeMax => OnLifeMaxChange(lifeMax))
            .AddTo(this);

        restUI.Heal.Subscribe(point => HealRatio(point, false)).AddTo(this);

        // Initialize life text and gauge
        restUI.OnLifeChange(status.Life.Value, status.LifeMax.Value);

        lifeGauge.UpdateLife(status.Life.Value, status.LifeMax.Value, false);
    }

    protected void OnLifeMaxChange(float lifeMax)
    {
        lifeGauge.UpdateLife(status.Life.Value, lifeMax, false);
    }

    public override bool HealRatio(float healRatio = 0f, bool isEffectOn = true, bool healAnyway = false)
    {
        if (!status.IsAlive || (mobStatus.IsLifeMax && !healAnyway)) return false;

        if (healRatio == 0f)
        {
            // Update RestUI life gauge
            restUI.OnLifeChange(status.Life.Value, status.LifeMax.Value);
            return false;
        }

        float heal = healRatio * status.LifeMax.Value;
        float lifeRatio = LifeRatio(status.Life.Value + heal);

        if (isEffectOn)
        {
            effect.OnHeal(healRatio);
            lifeGauge.OnHeal(healRatio, lifeRatio);
        }
        else if (status.Life.Value < status.LifeMax.Value)
        {
            lifeGauge.OnNoEffectHeal(heal, status.Life.Value, status.LifeMax.Value);
            if (lifeRatio == 1f) lifeGauge.OnLifeMax();
        }

        status.LifeChange(heal);
        return true;
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();

        // Update life text only.
        // Life gauge updating effects should be handled by OnDamage() or OnHealRatio() methods.
        lifeGauge.UpdateLifeText(life, status.LifeMax.Value);

        restUI.OnLifeChange(life, status.LifeMax.Value);
        anim.lifeRatio.Float = LifeRatio(life);

    }

    public override float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        float damage = base.Damage(attack, dir, type, attr);
        if (restUI.isActive) restUI.OnDamage();

        lifeGauge.OnDamage(LifeRatio(damage), LifeRatio(status.Life.Value));

        return damage;
    }

    protected override float CalcDamage(float attack, IDirection dir, AttackAttr attr = AttackAttr.None)
    {
        if (restUI.isActive) return Mathf.Max(mobStatus.CalcAttack(attack, null, attr), 0.0f);

        float shield = 0.0f;

        if (guardState.IsShieldOn(dir))
        {
            shield = mobStatus.Shield;
            guardState.SetShield();
        }

        return Mathf.Max(mobStatus.CalcAttack(attack, dir, attr) - shield, 0.0f);
    }
}
