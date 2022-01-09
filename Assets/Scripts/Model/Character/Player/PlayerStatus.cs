using UnityEngine;
using System.Collections.Generic;

public class PlayerStatus : MobStatus
{
    [SerializeField] protected MobData data;

    protected override void Awake()
    {
        param = data.Param(0);
        base.Awake();
    }

    public void SetPosition(KeyValuePair<Pos, IDirection> initPos)
    {
        var map = GameManager.Instance.worldMap;
        SetPosition(map.WorldPos(initPos.Key), initPos.Value);
    }
}
