using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus
{
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

    private HandEquipments handEquipments;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider>();

        // ResetStatus() is called inside InitParam() method.
        InitParam(Resources.Load<PlayerData>("DataAssets/Character/PlayerData").Param(0));
        handEquipments = new HandEquipments();
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

    public bool EquipR(ItemType type) => handEquipments.EquipR(type);
    public bool EquipL(ItemType type) => handEquipments.EquipL(type);

    public IObservable<EquipmentSource> EquipRObservable => handEquipments.SourceR;
    public IObservable<EquipmentSource> EquipLObservable => handEquipments.SourceL;
    public IObservable<IEquipments> FightStyleChange => handEquipments.CurrentEquipments;
}
