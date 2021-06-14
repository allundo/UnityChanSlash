using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePool))]
public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] protected ThirdPersonCamera mainCamera = default;
    [SerializeField] protected FightCircle fightCircle = default;
    [SerializeField] protected DoorHandler doorHandler = default;

    protected HidePool hidePool;
    protected CommandInput input;
    protected FightInput fightInput;
    protected DoorInput doorInput;

    private bool IsAttack => currentCommand is PlayerAttack;

    protected class FightInput
    {
        FightCircle fightCircle;

        Command jab;
        Command straight;
        Command kick;

        public FightInput(PlayerCommander commander)
        {
            this.fightCircle = commander.fightCircle;
            SetCommands(commander);
        }

        protected void SetCommands(PlayerCommander commander)
        {
            jab = new PlayerJab(commander, 0.6f);
            straight = new PlayerStraight(commander, 0.8f);
            kick = new PlayerKick(commander, 1.0f);

            fightCircle.JabButton.AttackSubject
                .Subscribe(_ => commander.InputReactive.Execute(jab))
                .AddTo(commander);

            fightCircle.StraightButton.AttackSubject
                .Subscribe(_ => commander.InputReactive.Execute(straight))
                .AddTo(commander);

            fightCircle.KickButton.AttackSubject
                .Subscribe(_ => commander.InputReactive.Execute(kick))
                .AddTo(commander);
        }
    }
    protected class DoorInput
    {
        DoorHandler doorHandler;

        Command forward;
        Command handle;

        PlayerAnimator anim;

        public DoorInput(PlayerCommander commander)
        {
            doorHandler = commander.doorHandler;
            anim = commander.anim as PlayerAnimator;
            SetCommands(commander);
        }

        protected void SetCommands(PlayerCommander commander)
        {
            forward = new ForwardCommand(commander, 1.0f);
            handle = new PlayerHandle(commander, 0.4f);

            doorHandler.ObserveGo
                .Subscribe(_ => { commander.InputReactive.Execute(forward); Debug.Log("Go"); })
                .AddTo(commander);

            doorHandler.ObserveHandle
                .Subscribe(_ => { commander.InputReactive.Execute(handle); Debug.Log("Handle"); })
                .AddTo(commander);

            doorHandler.ObserveHandOn
                .Subscribe(isHandOn => { anim.handOn.Bool = isHandOn; Debug.Log("HandOn: " + isHandOn); })
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
        die = new DieCommand(this, 8.0f);
        shieldOn = new PlayerShieldOn(this, 0.42f);

        input = new CommandInput(this);

        fightInput = new FightInput(this);
        doorInput = new DoorInput(this);
    }

    protected override Command GetCommand()
    {
        return input.GetCommand();
    }

    protected override void Update()
    {
        InputReactive.Execute(GetCommand());

        Tile forwardTile = map.ForwardTile;

        // Is face to enemy
        if (forwardTile.IsCharactorOn)
        {
            if (IsFightValid) guardState.SetEnemyDetected(true);
            fightCircle.SetActive(IsFightValid || IsAttack);
        }
        else
        {
            guardState.SetEnemyDetected(false);
            fightCircle.Inactivate();
        }

        if (!fightCircle.isActive && forwardTile is Door)
        {
            doorHandler.Activate((forwardTile as Door).IsOpen);
        }
        else
        {
            doorHandler.Inactivate();
        }
    }

    public override void SetDie()
    {
        doorHandler.Inactivate();
        fightCircle.Inactivate();

        base.SetDie();
    }

    public override void EnqueueShieldOn() { EnqueueCommand(shieldOn, true); }

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
