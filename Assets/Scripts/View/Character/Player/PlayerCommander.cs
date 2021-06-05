using UnityEngine;
using UnityEngine.EventSystems;
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
    protected FightInput fightInput;

    private bool IsFaceToEnemy => map.IsCharactorOn(map.GetForward);
    private bool IsAttack => currentCommand is PlayerAttack;

    protected class FightInput
    {
        FightCircle fightCircle;

        Command jab;
        Command straight;

        public FightInput(PlayerCommander commander)
        {
            this.fightCircle = commander.fightCircle;
            SetCommands(commander);
        }

        protected void SetCommands(PlayerCommander commander)
        {
            jab = new PlayerJab(commander, 0.6f);
            straight = new PlayerStraight(commander, 0.8f);

            fightCircle.JabButton.AttackSubject
                .Subscribe(_ => commander.InputReactive.Execute(jab))
                .AddTo(commander);

            fightCircle.StraightButton.AttackSubject
                .Subscribe(_ => commander.InputReactive.Execute(straight))
                .AddTo(commander);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        hidePool = GetComponent<HidePool>();
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 10.0f);
        input = new CommandInput(this);
        fightInput = new FightInput(this);
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

    protected class InputManager2
    {
        public bool isPressed { get; protected set; } = false;
        protected Command mainCommand;

        public InputManager2(Command mainCommand)
        {
            this.mainCommand = mainCommand;
        }

        public virtual Command FingerDown()
        {
            isPressed = true;
            return mainCommand;
        }

        public virtual Command FingerMove(Vector2 moveVec)
        {
            return mainCommand;
        }
        public virtual Command FingerUp()
        {
            isPressed = false;
            return mainCommand;
        }

        public virtual void Reset()
        {
            isPressed = false;
        }
    }

}
