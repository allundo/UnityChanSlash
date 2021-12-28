using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

/// <summary>
/// Convert player input UIs operation into a Command and enqueue it to PlayerCommander.<br />
/// In addition to MobInput, there is 'Trigger type' input implemented to improve usability.<br />
/// </summary>
[RequireComponent(typeof(PlayerCommandTarget))]
[RequireComponent(typeof(MapUtil))]
public class PlayerInput : ShieldInput
{
    // Fight UI to fight against Enemy on front Tile
    [SerializeField] protected FightCircle fightCircle = default;

    // Handling UIs to handle Door or Item on front Tile
    [SerializeField] protected DoorHandler doorHandler = default;
    [SerializeField] protected ItemHandler itemHandler = default;
    [SerializeField] protected ItemInventory itemInventory = default;
    [SerializeField] protected HandleIcon handleIcon = default;

    // Moving UIs: Normal type input
    // Those type of moving Commands are inputted continuously by long push on UI.
    [SerializeField] protected ForwardUI forwardUI = default;
    [SerializeField] protected PointerEnterUI rightUI = default;
    [SerializeField] protected PointerEnterUI leftUI = default;
    [SerializeField] protected PointerEnterUI backwardUI = default;

    // Action UIs: Trigger type input
    // Those type of action Commands interrupt series of moving Commands and applied once by one tap on UI.
    [SerializeField] protected PointerDownUI turnRUI = default;
    [SerializeField] protected PointerDownUI turnLUI = default;
    [SerializeField] protected PointerDownUI jumpUI = default;
    [SerializeField] protected PointerEnterUI guardUI = default;

    [SerializeField] protected Button restButton = default;

    [SerializeField] protected GameObject uiMask = default;

    protected PlayerCommandTarget playerTarget;
    protected bool IsAttack => commander.currentCommand is PlayerAttack;
    protected bool IsDash => commander.currentCommand is PlayerDash;
    protected bool IsForward => commander.currentCommand is PlayerForward;

    private IReactiveProperty<bool> isEnemyDetected = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsEnemyDetected => isEnemyDetected;

    /// <summary>
    /// Stops Trigger type input if false.
    /// Trigger type input is validated earlyer than Normal type input<br />
    /// but invalidated while moving Commands are executed continuously.
    /// </summary>
    protected bool isTriggerValid = true;

    /// <summary>
    /// Visible flag of all input UIs.
    /// </summary>
    protected bool isInputVisible = false;

    // Reserved Command input applied by other classes.
    // FIXME: need to implement game events handling system.
    public void EnqueueDropFloor() => Interrupt(new PlayerDropFloor(playerTarget, 220f));
    public void EnqueueStartMessage(MessageData data) => ForceEnqueue(new PlayerMessage(playerTarget, data));
    public void EnqueueRestartMessage(MessageData data) => Interrupt(new PlayerMessage(playerTarget, data));

    protected override void SetCommander()
    {
        playerTarget = target as PlayerCommandTarget;
        commander = new PlayerCommander(playerTarget);
    }

    protected override void SetCommands()
    {
        die = new PlayerDie(playerTarget, 288f);
    }

    protected override void SetInputs()
    {
        guardState = new PlayerGuardState(this);

        InitFightInput();
        InitHandleInput();
        InitMoveInput();

        SetInputVisible(isInputVisible);
    }

    protected override void Update()
    {
        if (commander.currentCommand is PlayerDie) return;

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
        bool isTriggerValid = this.isTriggerValid;
        this.isTriggerValid = false; // Disable Trigger input UI

        if (!isTriggerValid || !isCommandValid || cmd == null) return;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
    }

    /// <summary>
    /// Input a Command to PlayerCommander while isTriggerValid flag allows.
    /// </summary>
    public void InputTrigger(Command cmd)
    {
        if (!isTriggerValid || cmd == null) return;

        isCommandValid = !isCommandValid;
        isTriggerValid = !isTriggerValid;

        commander.EnqueueCommand(cmd);
    }

    /// <summary>
    /// Set validation of both of Normal and Trigger input.
    /// </summary>
    /// <param name="isTriggerOnly">Start Trigger only validated mode if true</param>
    public override void ValidateInput(bool isTriggerOnly = false)
    {
        // Is Trigger input already done on Trigger only validated mode?
        if (!isTriggerValid && isCommandValid)
        {
            isCommandValid = false;
            return;
        }

        isTriggerValid = true;
        isCommandValid = !isTriggerOnly;
    }

    /// <summary>
    /// Disables both of Normal and Trigger input.
    /// </summary>
    /// <param name="isTriggerOnly">Disables only Trigger input if true</param>
    public override void DisableInput(bool isTriggerOnly = false)
    {
        isTriggerValid = false;
        isCommandValid = isTriggerOnly && isCommandValid;
    }

    public override void ClearAll(bool isValidInput = false)
    {
        commander.ClearAll();
        isTriggerValid = isCommandValid = isValidInput;
    }

    /// <summary>
    /// Switch visibilities of all input UIs.
    /// </summary>
    public void SetInputVisible(bool isVisible = true)
    {
        isInputVisible = isVisible;
        restButton.gameObject.SetActive(isVisible);
        if (!isVisible) InactivateUIs();
    }

    public override void InputDie()
    {
        base.InputDie();
        SetInputVisible(false);
    }

    /// <summary>
    /// Switches display of input UIs according to the situation for every frames.
    /// </summary>
    private void DisplayInputUIs()
    {
        if (!isInputVisible) return;

        ITile forwardTile = map.ForwardTile;

        // Is face to enemy
        if (forwardTile.IsEnemyOn)
        {
            if (IsFightValid) isEnemyDetected.Value = true;
            fightCircle.SetActive(IsFightValid || IsAttack, forwardTile.OnEnemy);
            fightCircle.isForwardMovable = forwardTile.IsEnterable();
        }
        else
        {
            isEnemyDetected.Value = false;
            fightCircle.Inactivate();
        }

        bool isFaceToDoor = !IsDash && !fightCircle.isActive && forwardTile is Door && !forwardTile.IsItemOn;

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

        bool isFaceToItem = !IsDash && !fightCircle.isActive && forwardTile.IsItemOn;
        if (isFaceToItem)
        {
            itemHandler.Activate();
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            itemHandler.Inactivate();
        }

        if (!isFaceToDoor && !isFaceToItem && !itemInventory.IsPutItem)
        {
            forwardUI.Resize(1f, 1f);
            handleIcon.Disable();
        }

        bool isHandleUIOn = doorHandler.isPressed || itemHandler.isPressed || itemInventory.IsPutItem;

        uiMask.SetActive(isHandleUIOn || IsAttack || fightCircle.IsPressed);

        forwardUI.SetActive(forwardTile.IsEnterable(map.dir) && !isHandleUIOn);
        backwardUI.SetActive(map.IsBackwardMovable);
        rightUI.SetActive(map.IsRightMovable);
        leftUI.SetActive(map.IsLeftMovable);

        bool isTriggerActive = fightCircle.isActive || isTriggerValid || isCommandValid || IsShield;

        forwardUI.SetDashInputActive(IsForward || IsDash || isTriggerActive);
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
        // TODO: Refer duration and cancel time value from AttackButton object
        Command jab = new PlayerJab(playerTarget, 21.6f);
        Command straight = new PlayerStraight(playerTarget, 30f);
        Command kick = new PlayerKick(playerTarget, 43f);

        fightCircle.JabButton
            .Subscribe(_ => InputCommand(jab))
            .AddTo(this);

        fightCircle.StraightButton
            .Subscribe(_ => InputCommand(straight))
            .AddTo(this);

        fightCircle.KickButton
            .Subscribe(_ => InputCommand(kick))
            .AddTo(this);
    }

    /// <summary>
    /// Reserves handling Commands for each handling operation
    /// </summary>
    protected void InitHandleInput()
    {
        Command forward = new PlayerForward(playerTarget, 36f);
        Command handle = new PlayerHandle(playerTarget, 14.4f);
        Command getItem = new PlayerGetItem(playerTarget, 28.8f);
        PlayerPutItem putItem = new PlayerPutItem(playerTarget, 14.4f);

        Observable.Merge(doorHandler.ObserveGo, itemHandler.ObserveGo)
            .Subscribe(_ => InputCommand(forward))
            .AddTo(this);

        doorHandler.ObserveHandle
            .Subscribe(_ => InputTrigger(handle))
            .AddTo(this);

        PlayerAnimator anim = playerTarget.anim as PlayerAnimator;
        Observable.Merge(doorHandler.ObserveHandOn, itemHandler.ObserveHandOn, itemInventory.OnPutItem.Select(itemIcon => itemIcon != null))
            .Subscribe(isHandOn => anim.handOn.Bool = isHandOn)
            .AddTo(this);

        itemHandler.ObserveGet
            .Subscribe(_ => InputTrigger(getItem))
            .AddTo(this);

        itemInventory.OnPutApply
            .Subscribe(itemIcon => InputTrigger(putItem.SetItem(itemIcon)))
            .AddTo(this);

        itemInventory.OnUseItem
            .Subscribe(itemInfo => InputTrigger(new PlayerItem(playerTarget, itemInfo)))
            .AddTo(this);
    }

    /// <summary>
    /// Reserves move and action Commands for each move and action buttons.
    /// </summary>
    protected void InitMoveInput()
    {
        Command forward = new PlayerForward(playerTarget, 36f);
        Command right = new PlayerRight(playerTarget, 43f);
        Command left = new PlayerLeft(playerTarget, 43f);
        Command backward = new PlayerBack(playerTarget, 43f);
        Command run = new PlayerRun(playerTarget, 24f);
        Command startRunning = new PlayerStartRunning(playerTarget, 24f);
        var brake = new PlayerBrake(playerTarget, 48f);

        Command turnR = new PlayerTurnR(playerTarget, 18f);
        Command turnL = new PlayerTurnL(playerTarget, 18f);
        Command jump = new PlayerJump(playerTarget, 72f);

        Command guard = new GuardCommand(playerTarget, 1.44f);

        forwardUI.EnterObservable
            .Subscribe(_ => InputCommand(forward))
            .AddTo(this);

        forwardUI.DashObservable
            .Subscribe(isDashOn =>
            {
                if (isDashOn)
                {
                    // Input dash

                    if (commander.currentCommand is PlayerForward)
                    {
                        // Cancel forward command and start dash
                        Interrupt(startRunning);
                        return;
                    }

                    if (commander.currentCommand is PlayerRun)
                    {
                        // Reserve dash command to continue dash state
                        commander.ReplaceNext(run);
                        return;
                    }

                    // Reserve dash on command queue
                    InputTrigger(run);
                    return;
                }

                // Stop dash
                if (commander.NextCommand is PlayerRun && forwardUI.IsActive)
                {
                    // Replace reserved dash command with brake command
                    commander.ReplaceNext(brake);
                }
            })
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
            .AddTo(this);

        turnLUI.PressObservable
            .Subscribe(_ => InputTrigger(turnL))
            .AddTo(this);

        jumpUI.PressObservable
            .Subscribe(_ => InputTrigger(jump))
            .AddTo(this);

        guardUI.EnterObservable
            .Subscribe(_ => InputTrigger(guard))
            .AddTo(this);
    }

    public class PlayerGuardState : GuardState
    {
        private PlayerInput playerInput;
        private ShieldAnimator anim;
        private float timeToReady;
        public bool isShieldReady = false;

        private IObservable<bool> IsAutoGuardObservable => playerInput.IsEnemyDetected.SkipLatestValueOnSubscribe();
        private bool isAutoGuard => playerInput.IsEnemyDetected.Value;

        private Tween readyTween = null;
        private void SetShieldReady(bool isGuardOn)
        {
            anim.guard.Bool = isGuardOn;

            if (isGuardOn)
            {
                readyTween = DOVirtual.DelayedCall(timeToReady, () => isShieldReady = true, false).Play();
            }
            else
            {
                readyTween?.Kill();
                isShieldReady = false;
            }
        }

        public override bool IsShieldOn(IDirection attackDir) => input.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

        public PlayerGuardState(PlayerInput input, float duration = 15f, float timeToReady = 0.16f) : base(input, duration)
        {
            playerInput = input;
            this.timeToReady = timeToReady;
            anim = input.target.anim as ShieldAnimator;

            var IsShieldObservable = (input.commander as PlayerCommander)
                .CurrentObservable
                .Select(cmd => cmd is ShieldCommand);

            Observable.Merge(IsAutoGuardObservable, IsShieldObservable)
                .Select(_ => isAutoGuard || input.IsShield)
                .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
                .AddTo(input.target);
        }
    }
}
