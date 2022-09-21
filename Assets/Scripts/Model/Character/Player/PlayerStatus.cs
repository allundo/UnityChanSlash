using UnityEngine;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus
{
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

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
}
