using UnityEngine;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldCommander : MobCommander
{
    public GuardState guardState { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        guardState = GetComponent<GuardState>();
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
}
