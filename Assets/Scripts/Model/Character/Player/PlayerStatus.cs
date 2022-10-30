using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus
{
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

    public IObservable<EquipmentSource> EquipRObservable => equipR.SkipLatestValueOnSubscribe();
    public IObservable<EquipmentSource> EquipLObservable => equipL.SkipLatestValueOnSubscribe();

    private IReactiveProperty<EquipmentSource> equipR = new ReactiveProperty<EquipmentSource>();
    private IReactiveProperty<EquipmentSource> equipL = new ReactiveProperty<EquipmentSource>();

    public float AttackR => equipR.Value.attackGain;
    public float ShieldR => equipR.Value.shieldGain;
    public float AttackL => equipL.Value.attackGain;
    public float ShieldL => equipL.Value.shieldGain;

    private ItemInfo equipInfoR = null;
    private ItemInfo equipInfoL = null;

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

    public void EquipR(ItemInfo itemInfo)
    {
        equipInfoR = itemInfo;
        equipR.Value = ResourceLoader.Instance.GetEquipmentOrDefault(itemInfo);
    }
    public void EquipL(ItemInfo itemInfo)
    {
        equipInfoL = itemInfo;
        equipL.Value = ResourceLoader.Instance.GetEquipmentOrDefault(itemInfo);
    }
}
