using UnityEngine;


public abstract class ShieldCommand : Command
{
    protected GuardState guardState;

    public ShieldCommand(ShieldCommander commander, float duration) : base(commander, duration)
    {
        guardState = commander.guardState;
    }
}

public class GuardCommand : ShieldCommand
{
    public GuardCommand(ShieldCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        guardState.SetManualGuard(true);

        SetValidateTimer();
        SetDispatchFinal(() => guardState.SetManualGuard(false));
    }
}

public class ShieldOnCommand : ShieldCommand
{
    public ShieldOnCommand(ShieldCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        (anim as ShieldAnimator).shield.Fire();

        SetValidateTimer(0.1f);
        SetDispatchFinal();
    }
}
