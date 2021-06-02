using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] ThirdPersonCamera mainCamera = default;
    protected HidePool hidePool;
    protected CommandInput input;

    private bool IsFaceToEnemy => map.IsCharactorOn(map.GetForward);

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 10.0f);
        input = new CommandInput(this);
    }

    protected override Command GetCommand()
    {
        return input.GetCommand();
    }

    protected override void Update()
    {
        InputReactive.Execute(GetCommand());
        if (IsIdling) guardState.SetEnemyDetected(IsFaceToEnemy);
    }

}
