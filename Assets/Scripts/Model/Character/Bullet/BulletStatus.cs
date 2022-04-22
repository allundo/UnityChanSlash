using UnityEngine;

public class BulletStatus : MobStatus
{
    [SerializeField] protected BulletType type;

    public IStatus shotBy { get; protected set; }
    public virtual BulletStatus SetShooter(IStatus shooter)
    {
        shotBy = type == BulletType.HealSprit ? (shooter as BulletStatus).shotBy : shooter;
        attack = shooter.attack;
        return this;
    }

    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }

    public override float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None) => attack;
}
