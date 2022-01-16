using UnityEngine;

[RequireComponent(typeof(BulletCommandTarget))]
public class BulletInput : MobInput
{
    protected ICommand fire;
    protected ICommand moveForward;

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

    protected override ICommand GetCommand() => moveForward;
}
