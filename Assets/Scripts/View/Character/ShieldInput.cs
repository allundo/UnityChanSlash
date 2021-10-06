public abstract class ShieldInput : MobInput
{
    public GuardStateTemp guardState { get; protected set; }

    protected bool IsGuard => commander.currentCommand is GuardCommand;
    public override bool IsFightValid => IsIdling || IsGuard;

    protected override void SetCommands()
    {
        guardState = new GuardStateTemp(this);

        die = new DieCommand(target, 0.1f);
    }
}
