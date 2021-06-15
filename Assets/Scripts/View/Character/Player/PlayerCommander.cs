using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    private bool IsAttack => currentCommand is PlayerAttack;

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 8.0f);
        shieldOn = new PlayerShieldOn(this, 0.42f);

        SetInputs();
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

    public override void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }
}
