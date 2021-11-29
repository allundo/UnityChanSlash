public abstract class ShieldCommand : Command
{
    protected GuardState guardState;

    public ShieldCommand(CommandTarget target, GuardState guardState, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        this.guardState = guardState;
    }
}

public class GuardCommand : ShieldCommand
{
    public GuardCommand(CommandTarget target, GuardState guardState, float duration) : base(target, guardState, duration, 0.95f)
    { }

    protected override bool Action()
    {
        guardState.SetManualGuard(true);
        SetOnCompleted(() => guardState.SetManualGuard(false));
        return true;
    }
}

public class ShieldOnCommand : ShieldCommand
{
    public ShieldOnCommand(CommandTarget target, GuardState guardState, float duration) : base(target, guardState, duration, 0.1f)
    { }

    protected override bool Action()
    {
        (anim as ShieldAnimator).shield.Fire();
        return true;
    }
}
