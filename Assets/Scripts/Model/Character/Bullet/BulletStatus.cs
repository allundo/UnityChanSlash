using UnityEngine;

public class BulletStatus : MobStatus
{
    [SerializeField] protected MobData data;
    [SerializeField] protected BulletType type;

    protected override void Awake()
    {
        param = data.Param((int)type);
        base.Awake();
    }

    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }

    public override float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None) => attack;
}
