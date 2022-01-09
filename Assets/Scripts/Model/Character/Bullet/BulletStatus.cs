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

    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }

    public override float CalcAttack(float attack, IDirection attackDir) => attack;
}
