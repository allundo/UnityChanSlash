using UnityEngine;

public interface IBulletStatus : IStatus
{
    void SetAttack(float attack);
    IStatus shotBy { get; }
}

public class BulletStatus : Status, IBulletStatus
{
    [SerializeField] protected BulletType type;

    public IStatus shotBy { get; protected set; }
    public void SetAttack(float attack) => this.attack = attack;

    public virtual BulletStatus SetShooter(IStatus shooter)
    {
        shotBy = type == BulletType.HealSprit ? (shooter as IBulletStatus).shotBy : shooter;
        attack = shooter.attack;
        return this;
    }

    public override Status OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }
}
