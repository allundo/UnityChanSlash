using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    protected bool isTriggerValid
    {
        get
        {
            return IsTriggerValid.Value;
        }
        set
        {
            IsTriggerValid.Value = value;
        }
    }
    protected IReactiveProperty<bool> IsTriggerValid;
    protected ReactiveCommand<Command> InputTrigger;

    private bool IsAttack => currentCommand is PlayerAttack;

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();

        IsTriggerValid = new ReactiveProperty<bool>(true);
        InputTrigger = new ReactiveCommand<Command>(IsTriggerValid);
    }

    protected override void Start()
    {
        base.Start();
        InputTrigger.Subscribe(cmd => EnqueueCommand(cmd, IsIdling)).AddTo(this);
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 8.0f);
        shieldOn = new PlayerShieldOn(this, 0.42f);

        SetInputs();
    }

    protected void ExecuteCommand(Command cmd)
    {
        isTriggerValid = false;
        InputCommand.Execute(cmd);
    }

    protected override Command GetCommand() { return null; }

    protected override void Update()
    {
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
    }

    public override void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }
}
