public abstract class ShieldInput : MobInput
{
    public GuardState guardState { get; protected set; }
    protected Command shieldOn;
    public virtual void EnqueueShieldOn() { commander.EnqueueCommand(shieldOn, true); }

    protected override void SetCommands()
    {
        die = new DieCommand(commander, 0.1f);
        guardState = new GuardState(commander as ShieldCommander);
    }
}
