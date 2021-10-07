public abstract class ShieldInput : MobInput
{
    public GuardStateTemp guardState { get; protected set; }

    protected bool IsShield => commander.currentCommand is ShieldCommand;
    public override bool IsFightValid => IsIdling || IsShield;

    protected override void SetCommands()
    {
        guardState = new GuardStateTemp(this);

        die = new DieCommand(target, 0.1f);
    }
}
