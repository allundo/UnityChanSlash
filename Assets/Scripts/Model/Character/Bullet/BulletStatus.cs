using UnityEngine;

public class BulletStatus : MobStatus
{
    [SerializeField] protected MobData data;
    [SerializeField] protected int dataIndex = 0;

    protected override void Awake()
    {
        param = data.Param(dataIndex);
        base.Awake();
    }

    public override float CalcAttack(float attack, IDirection attackDir) => attack;
}
