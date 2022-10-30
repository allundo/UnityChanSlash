using UnityEngine;
using UniRx;
using static ShieldInput;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerEffect))]
[RequireComponent(typeof(PlayerAnimFX))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerFightStyle))]
public class PlayerReactor : MobReactor
{
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;
    [SerializeField] protected RestUI restUI = default;
    [SerializeField] protected HandEquipment handR = default;
    [SerializeField] protected HandEquipment handL = default;

    protected PlayerAnimator playerAnim;
    protected PlayerInput playerInput;
    protected PlayerStatus playerStatus;
    protected PlayerFightStyle playerFightStyle;
    protected ParticleSystem iceCrashVFX;

    public IAttacker lastAttacker { get; protected set; }
    public AttackType lastAttackType { get; protected set; }
    public string CauseOfDeath() => lastAttacker.CauseOfDeath(lastAttackType);

    protected GuardState guardState => playerInput.guardState;

    protected override void Awake()
    {
        base.Awake();
        playerAnim = anim as PlayerAnimator;
        playerInput = input as PlayerInput;
        playerStatus = status as PlayerStatus;
        playerFightStyle = fightStyle as PlayerFightStyle;
        iceCrashVFX = Resources.Load<ParticleSystem>("Prefabs/Effect/FX_ICE_CRASH");
    }

    protected override void Start()
    {
        status.Activate();
        playerAnim.lifeRatio.Float = 1f;

        base.Start();

        status.LifeMax
            .SkipLatestValueOnSubscribe()
            .Subscribe(lifeMax => OnLifeMaxChange(lifeMax))
            .AddTo(this);

        restUI.Heal.Subscribe(point => HealRatio(point, false)).AddTo(this);

        // Initialize life text and gauge
        restUI.OnLifeChange(status.Life.Value, status.LifeMax.Value);

        lifeGauge.UpdateLife(status.Life.Value, status.LifeMax.Value);

        playerStatus.EquipRObservable
            .Subscribe(source => handR.Equip(source))
            .AddTo(this);

        playerStatus.EquipLObservable
            .Subscribe(source => handL.Equip(source))
            .AddTo(this);

        var itemInventory = playerInput.GetItemInventory;

        itemInventory.EquipR
            .Subscribe(itemInfo => playerStatus.EquipR(itemInfo))
            .AddTo(this);

        itemInventory.EquipL
            .Subscribe(itemInfo => playerStatus.EquipL(itemInfo))
            .AddTo(this);

        itemInventory.FightStyleChange
             .Subscribe(equipments =>
             {
                 playerFightStyle.SetFightStyle(equipments);
                 playerInput.SetFightInput(equipments);
             })
             .AddTo(this);
    }

    protected void OnLifeMaxChange(float lifeMax)
    {
        lifeGauge.UpdateLife(status.Life.Value, lifeMax);
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
            lifeGauge.OnNoEffectHeal(heal, status.Life.Value);
            if (lifeRatio == 1f) lifeGauge.OnLifeMax();
        }

        status.LifeChange(heal);
        return true;
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();

        lifeGauge.UpdateLife(life, status.LifeMax.Value);

        restUI.OnLifeChange(life, status.LifeMax.Value);
        playerAnim.lifeRatio.Float = LifeRatio(life);
    }

    public override float Damage(IAttacker attacker, Attack.AttackData attackData)
    {
        lastAttacker = attacker;
        lastAttackType = attackData.type;
        return base.Damage(attacker, attackData);
    }

    public override float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        float damage = base.Damage(attack, dir, type, attr);
        if (restUI.isActive) restUI.OnDamage();

        lifeGauge.OnDamage(LifeRatio(damage));

        return damage;
    }

    protected override float CalcDamage(float attack, IDirection dir, AttackAttr attr = AttackAttr.None)
    {
        if (restUI.isActive) return Mathf.Max(mobStatus.CalcAttack(attack, null, attr), 0.0f);

        float shield = 0f;
        float minDamage = 0f;

        if (guardState.IsShieldOn(dir))
        {
            float shieldEffectiveness = guardState.SetShield();
            shield = (playerStatus.Shield + playerStatus.ShieldR * playerFightStyle.ShieldRatioR + playerStatus.ShieldL * playerFightStyle.ShieldRatioL) * shieldEffectiveness;

            if (shieldEffectiveness < 1f) minDamage = 0.1f;
        }

        return Mathf.Max(mobStatus.CalcAttack(attack, dir, attr) - shield, minDamage);
    }

    public void PitFall(float damage)
    {
        playerInput.InputPitFall(damage);
    }

    public override void OnFall()
    {
        bodyCollider.enabled = false;
        playerInput.SetInputVisible(false);
    }

    public override void OnWakeUp()
    {
        bodyCollider.enabled = true;
        playerInput.SetInputVisible(true, !(map as PlayerMapUtil).isInPit);
    }

    public override void OnDie()
    {
        map.ResetTile();
        effect.OnDie();
        playerInput.SetInputVisible(false);
        bodyCollider.enabled = false;
    }
}
