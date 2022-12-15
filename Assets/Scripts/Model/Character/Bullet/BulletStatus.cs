using UnityEngine;

public interface IBulletStatus : IStatus
{
    void SetAttack(float attack);
    IStatus shotBy { get; }
}

public class BulletStatus : Status, IBulletStatus
{
    public IStatus shotBy { get; protected set; }
    public void SetAttack(float attack) => this.attack = attack;
    public float Attack => attack * shotBy.MagicMultiplier;

    public virtual BulletStatus SetShooter(IStatus shooter)
    {
        shotBy = shooter;
        return this;
    }

    public override Status OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }
}
