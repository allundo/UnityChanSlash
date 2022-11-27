using UnityEngine;
using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus
{
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

    private IObservable<EquipmentSource> EquipObservable(int index) => equips[index].SkipLatestValueOnSubscribe();
    public IObservable<EquipmentSource> EquipRObservable => EquipObservable(2);
    public IObservable<EquipmentSource> EquipLObservable => EquipObservable(0);
    public IObservable<EquipmentSource> EquipBodyObservable => EquipObservable(1);

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


    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider>();

        // TODO: Refer to PlayerData to get level based player status.
        // ResetStatus() is called inside InitParam() method.
        InitParam(Resources.Load<PlayerData>("DataAssets/Character/PlayerData").Param(0));
    }

    protected override void OnActive()
    {
        // Don't call ResetStatus() on active
        activeSubject.OnNext(Unit.Default);
    }

    public void SetPosition(KeyValuePair<Pos, IDirection> initPos)
    {
        var map = GameManager.Instance.worldMap;
        SetPosition(map.WorldPos(initPos.Key), initPos.Value);
    }

    public void SetStatusData(float life, bool isHidden)
    {
        this.life.Value = life;
        this.isHidden = isHidden;
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
    private float CalcAttrDM(AttackAttr attr)
    {
        var attrDM = attrDamageMultiplier[attr];

        for (int index = 0; index < equips.Length; index++)
        {
            if (attr == Attribute(index)) attrDM -= ArmorGain(index);
        }

        return attrDM;
    }
}
