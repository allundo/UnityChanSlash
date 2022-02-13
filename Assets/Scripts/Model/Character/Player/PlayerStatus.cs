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
        param = data.Param(0);
        col = GetComponent<CapsuleCollider>();
        base.Awake();
    }

    public void SetPosition(KeyValuePair<Pos, IDirection> initPos)
    {
        var map = GameManager.Instance.worldMap;
        SetPosition(map.WorldPos(initPos.Key), initPos.Value);
    }
}
