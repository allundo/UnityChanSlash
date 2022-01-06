using UnityEngine;

public class BulletStatus : MobStatus
{
    [SerializeField] protected MobData data;
    [SerializeField] protected int dataIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        InitParam(data.Param(dataIndex));
    }

    public override float CalcAttack(float attack, IDirection attackDir) => attack;
}
