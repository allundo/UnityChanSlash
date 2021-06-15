using UnityEngine;
using UniRx;

public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] protected FightCircle fightCircle = default;
    [SerializeField] protected DoorHandler doorHandler = default;
    [SerializeField] protected PointerEnterUI forwardUI = default;
    [SerializeField] protected PointerEnterUI rightUI = default;
    [SerializeField] protected PointerEnterUI leftUI = default;
    [SerializeField] protected PointerEnterUI backwardUI = default;
    [SerializeField] protected PointerDownUI turnRUI = default;
    [SerializeField] protected PointerDownUI turnLUI = default;
    [SerializeField] protected PointerDownUI jumpUI = default;
    [SerializeField] protected PointerEnterUI guardUI = default;

    protected HidePool hidePool;
    protected FightInput fightInput;
    protected DoorInput doorInput;
    protected MoveInput moveInput;

    private void SetInputs()
    {
        fightInput = new FightInput(this);
        doorInput = new DoorInput(this);
        moveInput = new MoveInput(this);
    }

    private void CommandInput()
    {
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
            Door door = forwardTile as Door;

            doorHandler.SetActive(door.IsControllable, door.IsOpen);
        }
        else
        {
            doorHandler.Inactivate();
        }

        forwardUI.SetActive(forwardTile.IsEnterable());
        backwardUI.SetActive(map.IsBackwardMovable);
        rightUI.SetActive(map.IsRightMovable);
        leftUI.SetActive(map.IsLeftMovable);

        turnRUI.SetActive(isCommandValid);
        turnLUI.SetActive(isCommandValid);
        jumpUI.SetActive(isCommandValid);

        guardUI.SetActive(!fightCircle.isActive);
    }

    private void InactivateUIs()
    {
        doorHandler.Inactivate();
        fightCircle.Inactivate();

        forwardUI.Inactivate();
        backwardUI.Inactivate();
        rightUI.Inactivate();
        leftUI.Inactivate();

        turnRUI.Inactivate();
        turnLUI.Inactivate();
        jumpUI.Inactivate();

        guardUI.Inactivate();
    }

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
                .Subscribe(_ => commander.InputReactive.Execute(forward))
                .AddTo(commander);

            doorHandler.ObserveHandle
                .Subscribe(_ => commander.InputReactive.Execute(handle))
                .AddTo(commander);

            doorHandler.ObserveHandOn
                .Subscribe(isHandOn => anim.handOn.Bool = isHandOn)
                .AddTo(commander);
        }
    }

    protected class MoveInput
    {
        Command forward;
        Command right;
        Command left;
        Command backward;
        Command turnR;
        Command turnL;
        Command jump;
        Command guard;

        PlayerAnimator anim;

        public MoveInput(PlayerCommander commander)
        {
            SetCommands(commander);
        }

        protected void SetCommands(PlayerCommander commander)
        {
            forward = new ForwardCommand(commander, 1.0f);
            right = new RightCommand(commander, 1.2f);
            left = new LeftCommand(commander, 1.2f);
            backward = new BackCommand(commander, 1.2f);

            turnR = new TurnRCommand(commander, 0.5f);
            turnL = new TurnLCommand(commander, 0.5f);
            jump = new JumpCommand(commander, 2.0f);

            guard = new GuardCommand(commander, 0.2f);

            commander.forwardUI.EnterObservable
                .Subscribe(_ => commander.InputReactive.Execute(forward))
                .AddTo(commander);

            commander.rightUI.EnterObservable
                .Subscribe(_ => commander.InputReactive.Execute(right))
                .AddTo(commander);

            commander.leftUI.EnterObservable
                .Subscribe(_ => commander.InputReactive.Execute(left))
                .AddTo(commander);

            commander.backwardUI.EnterObservable
                .Subscribe(_ => commander.InputReactive.Execute(backward))
                .AddTo(commander);

            commander.turnRUI.PressObservable
                .Subscribe(_ => commander.InputReactive.Execute(turnR))
                .AddTo(commander);

            commander.turnLUI.PressObservable
                .Subscribe(_ => commander.InputReactive.Execute(turnL))
                .AddTo(commander);

            commander.jumpUI.PressObservable
                .Subscribe(_ => commander.InputReactive.Execute(jump))
                .AddTo(commander);

            commander.guardUI.EnterObservable
                .Subscribe(_ => commander.InputReactive.Execute(guard))
                .AddTo(commander);
        }
    }
}
