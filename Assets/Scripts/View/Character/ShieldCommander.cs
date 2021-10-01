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

}
