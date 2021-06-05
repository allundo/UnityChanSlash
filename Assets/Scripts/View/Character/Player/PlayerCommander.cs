using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] protected ThirdPersonCamera mainCamera = default;
    [SerializeField] protected FightCircle fightCircle = default;

    protected HidePool hidePool;
    protected CommandInput input;

    protected Command attack;
    protected Command attackStraight;

    private bool IsFaceToEnemy => map.IsCharactorOn(map.GetForward);
    private bool IsAttack => currentCommand == attackStraight;

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 10.0f);
        attack = new PlayerJab(this, 0.6f);
        attackStraight = new PlayerStraight(this, 0.8f);
        input = new CommandInput(this);

        fightCircle.AttackSubject.Subscribe(_ => InputReactive.Execute(attackStraight)).AddTo(this);
    }

    protected override Command GetCommand()
    {
        return input.GetCommand();
    }

    protected override void Update()
    {
        InputReactive.Execute(GetCommand());

        if (IsFaceToEnemy)
        {
            if (IsIdling) guardState.SetEnemyDetected(true);
            fightCircle.SetActive(IsIdling || IsAttack);
        }
        else
        {
            guardState.SetEnemyDetected(false);
            fightCircle.Inactivate();
        }
    }

}
