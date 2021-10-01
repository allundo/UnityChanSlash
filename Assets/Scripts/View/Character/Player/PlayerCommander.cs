using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePlateHandler))]
public partial class PlayerCommander : ShieldCommander
{
    protected HidePlateHandler hidePlateHandler;
    protected bool isTriggerValid = true;

    private bool IsAttack => currentCommand is PlayerAttack;

    protected ISubject<Unit> onClearAll = new Subject<Unit>();
    protected IObservable<Unit> OnClearAll => onClearAll;

    protected ISubject<bool> onUIVisible = new Subject<bool>();
    protected IObservable<bool> OnUIVisible => onUIVisible;

    public void EnqueueDropFloor() { EnqueueCommand(new PlayerDropFloor(this, 6.11f), true); }
    public void EnqueueStartMessage() { EnqueueCommand(new PlayerStartMessage(this, 0.1f)); }
    public void EnqueueRestartMessage() { EnqueueCommand(new PlayerRestartMessage(this, 0.1f), true); }

    protected override void Awake()
    {
        base.Awake();
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }

    protected override void Subscribe()
    {
        OnCompleted.Subscribe(_ => DispatchCommand()).AddTo(this);
        OnValidated.Subscribe(isTriggerOnly => ValidateInput(isTriggerOnly)).AddTo(this);
        OnUIVisible.Subscribe(isVisible => SetInputVisible(isVisible)).AddTo(this);
        OnClearAll.Subscribe(_ => cmdQueue.Clear()).AddTo(this);
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

    protected void ValidateInput(bool isTriggerOnly)
    {

        isTriggerValid = true;

        if (isTriggerOnly) return;

        isCommandValid = true;
    }

    protected override void InvalidateInput()
    {
        isCommandValid = false;
        isTriggerValid = false;
        currentCommand?.CancelValidateTween();
    }

    public override void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }
}
