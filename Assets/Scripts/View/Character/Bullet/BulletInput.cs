using UnityEngine;

[RequireComponent(typeof(BulletCommandTarget))]
public class BulletInput : MobInput
{
    protected Command fire;
    protected Command moveForward;

    protected override void SetCommands()
    {
        var bulletTarget = target as BulletCommandTarget;

        fire = new BulletFire(bulletTarget, 1f);
        moveForward = new BulletMove(bulletTarget, 1f);
        die = new BulletDie(bulletTarget, 1f);
    }

    public override void OnActive()
    {
        ForceEnqueue(fire, false);
    }

    protected override Command GetCommand() => moveForward;
}
