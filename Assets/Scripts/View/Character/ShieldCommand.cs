public abstract class ShieldCommand : Command
{
    protected GuardState guardState;

    public ShieldCommand(ShieldCommander commander, float duration, GuardState guardState) : base(commander, duration)
    {
        this.guardState = guardState;
    }
}

public class GuardCommand : ShieldCommand
{
    public GuardCommand(ShieldCommander commander, float duration, GuardState guardState) : base(commander, duration, guardState)
    { }

    public override void Execute()
    {
        guardState.SetManualGuard(true);

        SetValidateTimer();
        SetDispatchFinal(() => guardState.SetManualGuard(false));
    }
}

public class ShieldOnCommand : ShieldCommand
{
    public ShieldOnCommand(ShieldCommander commander, float duration, GuardState guardState) : base(commander, duration, guardState)
    { }

    public override void Execute()
    {
        (anim as ShieldAnimator).shield.Fire();

        SetValidateTimer(0.1f);
        SetDispatchFinal();
    }
}
