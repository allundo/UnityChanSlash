using UnityEngine;
using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

public interface IGetExp
{
    void AddExp(float expObtain, EnemyType type = EnemyType.None);
}

public class PlayerShooter : Shooter, IGetExp
{
    private PlayerStatus status;

    public PlayerShooter(float attack, PlayerStatus status) : base(attack, status)
    {
        this.status = status;
    }

    public void AddExp(float expObtain, EnemyType type = EnemyType.None) => status.AddExp(expObtain, type);
    public void IncMagic(AttackAttr attr = AttackAttr.None) => status.counter.IncMagic(attr);
}

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus, IGetExp
{
    private static readonly float EXP_GAIN_RATIO = 1.25f;
    private IReactiveProperty<float> exp = new ReactiveProperty<float>(0f);
    public IObservable<float> ExpChange => exp;
    public float Exp => exp.Value;
    private float expToNextLevel;
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

    private IObservable<EquipmentSource> EquipObservable(int index) => equips[index].SkipLatestValueOnSubscribe();
    public IObservable<EquipmentSource> EquipRObservable => EquipObservable(2);
    public IObservable<EquipmentSource> EquipLObservable => EquipObservable(0);
    public IObservable<EquipmentSource> EquipBodyObservable => EquipObservable(1);

    private ISubject<Unit> initSubject = new BehaviorSubject<Unit>(Unit.Default);
    public IObservable<DispStatus> StatusChange =>
        Observable.Merge(
            EquipRObservable.Select(_ => Unit.Default),
            EquipLObservable.Select(_ => Unit.Default),
            EquipBodyObservable.Select(_ => Unit.Default),
            initSubject
        )
        .Select(_ => GetDispStatus());

    /// <summary>
    /// [0: Left hand weapon], 
    /// [1: Body accessory], 
    /// [2: Right hand weapon]
    /// </summary>
    private IReactiveProperty<EquipmentSource>[] equips = new IReactiveProperty<EquipmentSource>[3]
        .Select(equip => new ReactiveProperty<EquipmentSource>()).ToArray();

    private float AttackGain(int index) => equips[index].Value.attackGain;
    private float ShieldGain(int index) => equips[index].Value.shieldGain;
    private float ArmorGain(int index) => equips[index].Value.armorGain;
    private float ArmorSum => equips.Sum(equip => equip.Value.armorGain);
    private AttackAttr Attribute(int index) => equips[index].Value.attribute;
    public float AttackR => AttackGain(2);
    public float ShieldR => ShieldGain(2);
    public float AttackL => AttackGain(0);
    public float ShieldL => ShieldGain(0);

    /// <summary>
    /// Item info of equipments - 
    /// [0: Left hand weapon], 
    /// [1: Body accessory], 
    /// [2: Right hand weapon]
    /// </summary>
    private ItemInfo[] equipInfo = new ItemInfo[3];

    public PlayerCounter counter { get; private set; }
    public ClassSelector selector { get; private set; }
    private PlayerFightStyle fightStyle;
    public override float Shield => base.Shield * (1f + ShieldR * fightStyle.ShieldRatioR + ShieldL * fightStyle.ShieldRatioL);

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider>();
        fightStyle = GetComponent<PlayerFightStyle>();

        level = 0;
        counter = new PlayerCounter();
        selector = new ClassSelector();
        levelGain = selector.SetSelector(LevelGainType.Balance);

        // ResetStatus() is called inside InitParam() method.
        InitParam(Resources.Load<PlayerData>("DataAssets/Character/PlayerData").Param(0));
    }

    public void SetPosition(WorldMap map, KeyValuePair<Pos, IDirection> initPos)
    {
        SetPosition(map.WorldPos(initPos.Key), initPos.Value);
    }

    public void SetStatusData(float life, int level, float exp, bool isHidden, PlayerCounter counter, LevelGainType type)
    {
        this.isHidden = isHidden;
        this.exp.Value = exp;
        this.counter = counter;
        levelGain = selector.SetSelector(type);
        InitParam(param, new MobStatusStoreData(level, life));
    }

    public override IStatus InitParam(Param param, StatusStoreData data = null)
    {
        base.InitParam(param, data);
        expToNextLevel = mobParam.baseExp * Mathf.Pow(EXP_GAIN_RATIO, level);
        initSubject.OnNext(Unit.Default);
        return this;
    }

    public void AddExp(float expObtain, EnemyType type = EnemyType.None)
    {
        counter.IncDefeat(type);

        // Don't get exp on dying.
        if (!IsAlive) return;

        exp.Value += expObtain;

        if (exp.Value >= expToNextLevel)
        {
            exp.Value -= expToNextLevel;
            expToNextLevel *= EXP_GAIN_RATIO;

            var prevLifeMax = lifeMax.Value;

            levelGain = selector.SelectType(counter);
            counter.TotalCounts();
            InitParam(param, new MobStatusStoreData(level + 1, life.Value));

            life.Value += Mathf.Max(lifeMax.Value - prevLifeMax, 0f);

            ActiveMessageController.Instance.LevelUp(level);
        }
    }

    /// <summary>
    /// Set equipment info source as reference of attack and defense powers.
    /// </summary>
    /// <param name="index">[0: Left hand], [1: Body accessory], [2: Right hand]</param>
    /// <param name="itemInfo"></param>
    private void Equip(int index, ItemInfo itemInfo)
    {
        equipInfo[index] = itemInfo;
        equips[index].Value = ResourceLoader.Instance.GetEquipmentOrDefault(itemInfo);
    }

    public void EquipR(ItemInfo itemInfo) => Equip(2, itemInfo);
    public void EquipL(ItemInfo itemInfo) => Equip(0, itemInfo);
    public void EquipBody(ItemInfo itemInfo) => Equip(1, itemInfo);

    public override float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None)
    {
        var attrDM = CalcAttrDM(attr);

        // Direction doesn't affect attack power when absorbing attribute of the attack.
        var dirDM = attrDM > 0f ? dirDamageMultiplier[GetDamageType(attackDir)] : 1f;

        return Mathf.Max(attack * (ArmorMultiplier - ArmorSum * 0.25f) * dirDM * attrDM, 1f);
    }

    /// <summary>
    /// Reduce damage if equipment attribute is the same with the damage attribute.
    /// </summary>
    /// <param name="attr">damage attribute</param>
    /// <returns>damage multiplier based on the attribute</returns>
    protected override float CalcAttrDM(AttackAttr attr)
    {
        var attrDM = base.CalcAttrDM(attr);

        for (int index = 0; index < equips.Length; index++)
        {
            if (attr == Attribute(index)) attrDM -= ArmorGain(index);
        }

        return attrDM;
    }

    public void TryIncShield()
    {
        if (equips[0].Value.category == EquipmentCategory.Shield)
        {
            counter.IncShield();
        }
    }

    public (int attack, int magic) GetAttackMagic()
    {
        var disp = GetDispStatus();
        return ((int)disp.attack, (int)disp.magic);
    }

    private DispStatus GetDispStatus()
    {
        return new DispStatus()
        {
            level = this.level + 1,
            exp = this.exp.Value,
            expToNextLevel = this.expToNextLevel,
            levelGainTypeName = levelGain.name,
            attack = this.attack,
            equipR = AttackR * 100f,
            equipL = AttackL * 100f,
            armor = (1f - ArmorMultiplier) * 100f,
            shield = Shield,
            magic = (MagicMultiplier - 1f) * 100f,
            resistFire = (1f - CalcAttrDM(AttackAttr.Fire)) * 100f,
            resistIce = (1f - CalcAttrDM(AttackAttr.Ice)) * 100f,
            resistDark = (1f - CalcAttrDM(AttackAttr.Dark)) * 100f,
            resistLight = (1f - CalcAttrDM(AttackAttr.Light)) * 100f,
        };
    }
}
