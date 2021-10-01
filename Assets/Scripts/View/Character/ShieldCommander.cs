using UnityEngine;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldCommander : MobCommander
{
    public GuardState guardState { get; protected set; }

    public bool IsFightValid => currentCommand == null || currentCommand is GuardCommand;

    protected Command shieldOn = null;
    public virtual void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }

    protected override void Awake()
    {
        base.Awake();
        guardState = GetComponent<GuardState>();
    }

    protected override void SetCommands()
    {
        base.SetCommands();
        shieldOn = new ShieldOnCommand(this, 0.8f);
    }

    protected abstract class ShieldCommand : Command
    {
        protected GuardState guardState;

        public ShieldCommand(ShieldCommander commander, float duration) : base(commander, duration)
        {
            guardState = commander.guardState;
        }
    }

    protected class GuardCommand : ShieldCommand
    {
        public GuardCommand(ShieldCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            guardState.SetManualGuard(true);

            SetValidateTimer();
            SetDispatchFinal(() => guardState.SetManualGuard(false));
        }
    }

    protected class ShieldOnCommand : ShieldCommand
    {
        public ShieldOnCommand(ShieldCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            (anim as ShieldAnimator).shield.Fire();

            SetValidateTimer(0.1f);
            SetDispatchFinal();
        }
    }
}
