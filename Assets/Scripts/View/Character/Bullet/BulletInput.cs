using UnityEngine;

[RequireComponent(typeof(BulletCommandTarget))]
public class BulletInput : MobInput
{
    protected Command fire;
    protected Command moveForward;

    protected override void SetCommands()
    {
        var bulletTarget = target as BulletCommandTarget;

        fire = new BulletFire(bulletTarget, 0.8f);
        moveForward = new BulletMove(bulletTarget, 0.8f);
        die = new BulletDie(bulletTarget, 0.8f);
    }

    public override void OnActive()
    {
        ForceEnqueue(fire, true);
    }

    protected override Command GetCommand() => moveForward;
}
