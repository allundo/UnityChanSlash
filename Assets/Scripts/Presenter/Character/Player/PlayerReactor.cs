using UnityEngine;
using UniRx;
using static ShieldInput;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerEffect))]
[RequireComponent(typeof(PlayerAnimFX))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerMapUtil))]
[RequireComponent(typeof(PlayerFightStyle))]
public class PlayerReactor : MobReactor
{
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;
    [SerializeField] protected RestUI restUI = default;
    [SerializeField] protected HandREquipment handR = default;
    [SerializeField] protected HandLEquipment handL = default;
    [SerializeField] protected NeckEquipment neck = default;
    [SerializeField] protected StatusUI statusUI = default;

    protected PlayerAnimator playerAnim;
    protected PlayerInput playerInput;
    protected PlayerStatus playerStatus;
    protected PlayerEffect playerEffect;
    protected PlayerMapUtil playerMap;
    protected ParticleSystem iceCrashVFX;

    protected FloorIllness floorIllness;

    public string CauseOfDeath() => lastAttacker.CauseOfDeath(lastAttackType);

    protected GuardState guardState => playerInput.guardState;

    protected override void Awake()
    {
        base.Awake();
        playerAnim = anim as PlayerAnimator;
        playerInput = input as PlayerInput;
        playerStatus = status as PlayerStatus;
        playerEffect = effect as PlayerEffect;
        playerMap = map as PlayerMapUtil;
        iceCrashVFX = Resources.Load<ParticleSystem>("Prefabs/Effect/FX_ICE_CRASH");

        floorIllness = new FloorIllness(lifeGauge, restUI);
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
        restUI.Poison.Subscribe(point => PoisonRatio(point)).AddTo(this);

        // Initialize life text and gauge
        restUI.OnLifeChange(status.Life.Value, status.LifeMax.Value);

        lifeGauge.UpdateLife(status.Life.Value, status.LifeMax.Value);

        playerStatus.EquipRObservable
            .Subscribe(source => handR.Equip(source))
            .AddTo(this);

        playerStatus.EquipLObservable
            .Subscribe(source => handL.Equip(source))
            .AddTo(this);

        playerStatus.EquipBodyObservable
            .Subscribe(source => neck.Equip(source))
            .AddTo(this);

        var itemInventory = ItemInventory.Instance;

        itemInventory.EquipR
            .Subscribe(itemInfo => playerStatus.EquipR(itemInfo))
            .AddTo(this);

        itemInventory.EquipL
            .Subscribe(itemInfo => playerStatus.EquipL(itemInfo))
            .AddTo(this);

        itemInventory.EquipBody
            .Subscribe(itemInfo => playerStatus.EquipBody(itemInfo))
            .AddTo(this);

        itemInventory.FightStyleChange
             .Subscribe(equipments =>
             {
                 (fightStyle as PlayerFightStyle).SetFightStyle(equipments);
                 playerInput.SetFightInput(equipments);
                 statusUI.UpdateShield(playerStatus.ShieldSum);
             })
             .AddTo(this);

        playerStatus.ExpChange
            .Subscribe(exp => statusUI.UpdateExp(exp))
            .AddTo(this);

        playerStatus.StatusChange
            .Subscribe(status => statusUI.UpdateValues(status))
            .AddTo(this);
    }

    protected override void OnActive()
    {
        effect.OnActive();
        map.OnActive();
        input.OnActive();
    }

    public void SwitchFloor(int floor)
    {
        floorIllness.SwitchFloor(floor);
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

    public bool PoisonRatio(float poisonRatio = 0f)
    {
        float maxDamage = status.Life.Value - 1f;

        if (!status.IsAlive || maxDamage <= 0f) return false;

        lifeGauge.OnPoison(poisonRatio);
        status.LifeChange(-Mathf.Min(poisonRatio * status.LifeMax.Value, maxDamage));

        return true;
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InterruptDie();

        lifeGauge.UpdateLife(life, status.LifeMax.Value);

        restUI.OnLifeChange(life, status.LifeMax.Value);
        playerAnim.lifeRatio.Float = LifeRatio(life);
    }
    public override float Damage(IAttacker attacker, Attack.AttackData attackData)
    {
        var damage = base.Damage(attacker, attackData);

        if (attacker is Shooter)
        {
            playerStatus.counter.IncMagicDamage();
        }

        if (attacker is EnemyStatus)
        {
            playerStatus.counter.IncDamage();
        }

        return damage;
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
            shield = playerStatus.ShieldSum * shieldEffectiveness;

            playerStatus.TryIncShield();

            if (shieldEffectiveness >= 1f)
            {
                // Cancel damage count when shield is completely on
                playerStatus.counter.DecDamage();
            }
            else
            {
                minDamage = 1f;
            }
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
        playerInput.SetInputVisible(true, !playerMap.isInPit);
    }

    public override void OnDie()
    {
        playerMap.ResetTile();
        playerEffect.OnDie();
        playerInput.SetInputVisible(false);
        bodyCollider.enabled = false;
    }

    public void Iced(float framesToMelt, bool isPaused)
    {
        playerStatus.SetIcingFrames(framesToMelt);
        playerEffect.OnIced(playerStatus.corePos, isPaused);
    }

    public void IncPotion() => playerStatus.counter.IncPotion();
}
