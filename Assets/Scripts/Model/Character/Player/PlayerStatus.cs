using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStatus : MobStatus
{
    [SerializeField] protected MobData data;
    protected CapsuleCollider col;

    public override Vector3 corePos => transform.position + col.center;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider>();
        InitParam(Resources.Load<PlayerData>("DataAssets/Character/PlayerData").Param(0));
    }

    public void SetPosition(KeyValuePair<Pos, IDirection> initPos)
    {
        var map = GameManager.Instance.worldMap;
        SetPosition(map.WorldPos(initPos.Key), initPos.Value);
    }
}
