using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    protected HidePool hidePool;
    protected bool isTriggerValid = true;

    private bool IsAttack => currentCommand is PlayerAttack;

    public void EnqueueDropFloor() { EnqueueCommand(new PlayerDropFloor(this, 6.11f), true); }
    public void EnqueueStartMessage() { EnqueueCommand(new PlayerStartMessage(this, 0.1f)); }

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();
    }

    protected override void SetCommands()
    {
        die = new PlayerDie(this, 8.0f);
        shieldOn = new PlayerShieldOn(this, 0.42f);

        SetInputs();
    }

    private void ExecuteTrigger(Command cmd)
    {
        if (!isTriggerValid) return;
        EnqueueCommand(cmd, IsIdling);
    }

    protected override void Execute(Command cmd)
    {
        isTriggerValid = false;

        if (!isCommandValid) return;

        EnqueueCommand(cmd, IsIdling);
    }

    protected override Command GetCommand() { return null; }

    protected override void Update()
    {
        if (currentCommand is DieCommand) return;
        CommandInput();
    }

    public override void SetDie()
    {
        base.SetDie();
        InactivateUIs();
    }

    protected override void ValidateInput()
    {
        isCommandValid = true;
        isTriggerValid = true;
    }

    protected override void InvalidateInput()
    {
        isCommandValid = false;
        isTriggerValid = false;
        currentCommand?.CancelValidateTween();
    }

    public override void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }
}
