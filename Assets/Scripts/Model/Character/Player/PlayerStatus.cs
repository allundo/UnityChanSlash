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

    private ItemInfo equipInfoR = null;
    private ItemInfo equipInfoL = null;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider>();

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
        equipR.Value = GetEquipmentSource(itemInfo);
    }
    public void EquipL(ItemInfo itemInfo)
    {
        equipInfoL = itemInfo;
        equipL.Value = GetEquipmentSource(itemInfo);
    }

    private EquipmentSource GetEquipmentSource(ItemInfo itemInfo)
    {
        return ResourceLoader.Instance.GetEquipmentSource(itemInfo != null ? itemInfo.type : ItemType.Null);
    }
}
