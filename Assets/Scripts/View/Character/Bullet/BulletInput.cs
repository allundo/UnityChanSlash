using UnityEngine;

[RequireComponent(typeof(BulletCommandTarget))]
public class BulletInput : MobInput
{
    protected Command fire;
    protected Command moveForward;

    protected override void SetCommands()
    {
        var bulletTarget = target as BulletCommandTarget;

        fire = new BulletFire(bulletTarget, 28f);
        moveForward = new BulletMove(bulletTarget, 28f);
        die = new BulletDie(bulletTarget, 28f);
    }

    public override void OnActive()
    {
        Interrupt(fire);
    }

    protected override Command GetCommand() => moveForward;
}
