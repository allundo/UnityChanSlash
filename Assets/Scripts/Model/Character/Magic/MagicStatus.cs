using UnityEngine;

public interface IMagicStatus : IStatus
{
    void SetAttack(float attack);
    IStatus shotBy { get; }
}

public class MagicStatus : Status, IMagicStatus
{
    public IStatus shotBy { get; protected set; }
    public void SetAttack(float attack) => this.attack = attack;
    public float Attack => attack * shotBy.MagicMultiplier;

    public virtual MagicStatus SetShooter(IStatus shooter)
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
