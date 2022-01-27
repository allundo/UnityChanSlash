public abstract class ShieldCommand : Command
{
    public ShieldCommand(CommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    { }
}

public class GuardCommand : ShieldCommand
{
    public GuardCommand(CommandTarget target, float duration, float validateTiming = 0.3f) : base(target, duration, validateTiming)
    { }

    protected override bool Action() => true;
}

public class ShieldOnCommand : ShieldCommand
{
    public ShieldOnCommand(CommandTarget target, float duration, float validateTiming = 0.1f) : base(target, duration, validateTiming)
    { }

    protected override bool Action()
    {
        (anim as ShieldAnimator).shield.Fire();
        return true;
    }
}
