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

        InputFire();
    }

    public virtual void InputFire()
    {
        if (commander.IsIdling) ForceEnqueue(fire, true);
    }

    protected override Command GetCommand() => moveForward;

    public override void InputCommand(Command cmd)
    {
        if (!isCommandValid || cmd == null) return;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
    }
}
