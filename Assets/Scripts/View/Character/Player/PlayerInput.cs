using UnityEngine;
using UniRx;

/// <summary>
/// Convert player input UIs operation into a Command and enqueue it to PlayerCommander.<br />
/// In addition to MobInput, there is 'Trigger type' input implemented to improve usability.<br />
/// </summary>
[RequireComponent(typeof(PlayerCommander))]
[RequireComponent(typeof(MapUtil))]
public class PlayerInput : ShieldInput
{
    // Fight UI to fight against Enemy on front Tile
    [SerializeField] protected FightCircle fightCircle = default;

    // Handling UIs to handle Door or Item on front Tile
    [SerializeField] protected DoorHandler doorHandler = default;
    [SerializeField] protected ItemHandler itemHandler = default;
    [SerializeField] protected HandleIcon handleIcon = default;

    // Moving UIs: Normal type input
    // Those type of moving Commands are inputted continuously by long push on UI.
    [SerializeField] protected PointerEnterUI forwardUI = default;
    [SerializeField] protected PointerEnterUI rightUI = default;
    [SerializeField] protected PointerEnterUI leftUI = default;
    [SerializeField] protected PointerEnterUI backwardUI = default;

    // Action UIs: Trigger type input
    // Those type of action Commands interrupt series of moving Commands and applied once by one tap on UI.
    [SerializeField] protected PointerDownUI turnRUI = default;
    [SerializeField] protected PointerDownUI turnLUI = default;
    [SerializeField] protected PointerDownUI jumpUI = default;
    [SerializeField] protected PointerEnterUI guardUI = default;

    /// <summary>
    /// Target PlayerCommander to input Command. <br />
    /// Trigger type input is validated earlyer than Normal type input<br />
    /// but invalidated while moving Commands are executed continuously.
    /// </summary>
    protected PlayerCommander playerCommander;

    /// <summary>
    /// Stops Trigger type input if false.
    /// </summary>
    protected bool isTriggerValid = true;

    /// <summary>
    /// Visible flag of all input UIs.
    /// </summary>
    protected bool isInputVisible = true;

    // Reserved Command input applied by other classes.
    // FIXME: need to implement game events handling system.
    public void EnqueueDropFloor() => ForceEnqueue(new PlayerDropFloor(playerCommander, 6.11f), true);
    public void EnqueueStartMessage() => ForceEnqueue(new PlayerStartMessage(playerCommander, 0.1f), false);
    public void EnqueueRestartMessage() => ForceEnqueue(new PlayerRestartMessage(playerCommander, 0.1f), true);

    protected override void SetCommands()
    {
        playerCommander = commander as PlayerCommander;

        die = new PlayerDie(playerCommander, 8.0f);
        guardState = new GuardState(playerCommander, 0.42f);

        InitFightInput();
        InitHandleInput();
        InitMoveInput();
    }

    protected override void Subscribe()
    {
        commander.onValidateInput.Subscribe(isValid => ValidateInput(isValid)).AddTo(this);
        playerCommander.OnUIVisible.Subscribe(isVisible => SetInputVisible(isVisible)).AddTo(this);
        playerCommander.onValidateTrigger.Subscribe(_ => isTriggerValid = true).AddTo(this);
    }

    protected override void Update()
    {
        if (commander.IsDie) return;
        DisplayInputUIs();
    }

    protected override Command GetCommand() => null;

    /// <summary>
    /// Input a Command to PlayerCommander. <br />
    /// When Normal input is applied, Trigger input is invalidated.
    /// </summary>
    /// <param name="cmd">Command to input</param>
    public override void InputCommand(Command cmd)
    {
        isTriggerValid = false;

        if (!isCommandValid) return;

        commander.EnqueueCommand(cmd);
    }

    /// <summary>
    /// Input a Command to PlayerCommander while isTriggerValid flag allows.
    /// </summary>
    public void InputTrigger(Command cmd)
    {
        if (!isTriggerValid) return;

        commander.EnqueueCommand(cmd);
    }

    /// <summary>
    /// Switch visibilities of all input UIs.
    /// </summary>
    public void SetInputVisible(bool isVisible = true)
    {
        isInputVisible = isVisible;
        if (!isVisible) InactivateUIs();
    }

    /// <summary>
    /// Switches display of input UIs according to the situation for every frames.
    /// </summary>
    private void DisplayInputUIs()
    {
        if (!isInputVisible) return;

        ITile forwardTile = map.ForwardTile;

        // Is face to enemy
        if (forwardTile.IsCharactorOn)
        {
            bool isFightValid = playerCommander.IsFightValid;
            if (isFightValid) guardState.SetEnemyDetected(true);
            fightCircle.SetActive(isFightValid || playerCommander.IsAttack, forwardTile.OnCharacter);
        }
        else
        {
            guardState.SetEnemyDetected(false);
            fightCircle.Inactivate();
        }

        bool isFaceToDoor = !fightCircle.isActive && forwardTile is Door && !forwardTile.IsItemOn;

        if (isFaceToDoor)
        {
            Door door = forwardTile as Door;

            doorHandler.SetActive(door.IsControllable, door.IsOpen);
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            doorHandler.Inactivate();
        }

        bool isFaceToItem = !fightCircle.isActive && forwardTile.IsItemOn;
        if (isFaceToItem)
        {
            itemHandler.Activate();
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            itemHandler.Inactivate();
        }

        if (!isFaceToDoor && !isFaceToItem)
        {
            forwardUI.Resize(1f, 1f);
            handleIcon.Disable();
        }

        forwardUI.SetActive(forwardTile.IsEnterable(map.dir) && !doorHandler.isPressed && !itemHandler.isPressed);
        backwardUI.SetActive(map.IsBackwardMovable);
        rightUI.SetActive(map.IsRightMovable);
        leftUI.SetActive(map.IsLeftMovable);

        bool isTriggerActive = fightCircle.isActive || isTriggerValid || isCommandValid || playerCommander.IsGuard;

        turnRUI.SetActive(isTriggerActive, fightCircle.isActive);
        turnLUI.SetActive(isTriggerActive, fightCircle.isActive);
        jumpUI.SetActive(isTriggerActive, fightCircle.isActive);

        guardUI.SetActive(!fightCircle.isActive);
    }

    /// <summary>
    /// Inactivate all UIs
    /// </summary>
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

    /// <summary>
    /// Reserves fight behavior Commands for each attack button
    /// </summary>
    protected void InitFightInput()
    {
        Command jab = new PlayerJab(playerCommander, 0.6f);
        Command straight = new PlayerStraight(playerCommander, 0.85f);
        Command kick = new PlayerKick(playerCommander, 1.2f);

        fightCircle.JabButton.AttackSubject
            .Subscribe(_ => InputCommand(jab))
            .AddTo(commander);

        fightCircle.StraightButton.AttackSubject
            .Subscribe(_ => InputCommand(straight))
            .AddTo(commander);

        fightCircle.KickButton.AttackSubject
            .Subscribe(_ => InputCommand(kick))
            .AddTo(commander);
    }

    /// <summary>
    /// Reserves handling Commands for each handling operation
    /// </summary>
    protected void InitHandleInput()
    {
        Command forward = new PlayerForward(playerCommander, 1.0f);
        Command handle = new PlayerHandle(playerCommander, 0.4f);
        Command getItem = new PlayerGetItem(playerCommander, 0.8f);

        Observable.Merge(doorHandler.ObserveGo, itemHandler.ObserveGo)
            .Subscribe(_ => InputCommand(forward))
            .AddTo(commander);

        doorHandler.ObserveHandle
            .Subscribe(_ => InputTrigger(handle))
            .AddTo(commander);

        PlayerAnimator anim = commander.anim as PlayerAnimator;
        Observable.Merge(doorHandler.ObserveHandOn, itemHandler.ObserveHandOn)
            .Subscribe(isHandOn => anim.handOn.Bool = isHandOn)
            .AddTo(commander);

        itemHandler.ObserveGet
            .Subscribe(_ => InputTrigger(getItem))
            .AddTo(commander);
    }

    /// <summary>
    /// Reserves move and action Commands for each move and action buttons.
    /// </summary>
    protected void InitMoveInput()
    {
        Command forward = new PlayerForward(playerCommander, 1.0f);
        Command right = new PlayerRight(playerCommander, 1.2f);
        Command left = new PlayerLeft(playerCommander, 1.2f);
        Command backward = new PlayerBack(playerCommander, 1.2f);

        Command turnR = new PlayerTurnR(playerCommander, 0.5f);
        Command turnL = new PlayerTurnL(playerCommander, 0.5f);
        Command jump = new PlayerJump(playerCommander, 2.0f);

        Command guard = new GuardCommand(playerCommander, 0.02f, guardState);

        forwardUI.EnterObservable
            .Subscribe(_ => InputCommand(forward))
            .AddTo(this);

        rightUI.EnterObservable
            .Subscribe(_ => InputCommand(right))
            .AddTo(this);

        leftUI.EnterObservable
            .Subscribe(_ => InputCommand(left))
            .AddTo(this);

        backwardUI.EnterObservable
            .Subscribe(_ => InputCommand(backward))
            .AddTo(this);

        turnRUI.PressObservable
            .Subscribe(_ => InputTrigger(turnR))
            .AddTo(commander);

        turnLUI.PressObservable
            .Subscribe(_ => InputTrigger(turnL))
            .AddTo(commander);

        jumpUI.PressObservable
            .Subscribe(_ => InputTrigger(jump))
            .AddTo(commander);

        guardUI.EnterObservable
            .Subscribe(_ => InputTrigger(guard))
            .AddTo(commander);
    }

    /// <summary>
    /// Set validation of both of Normal and Trigger input.
    /// </summary>
    /// <param name="isValid">true: validate, false: invalidate</param>
    public override void ValidateInput(bool isValid)
    {
        isCommandValid = isValid;
        isTriggerValid = isValid;
    }
}
