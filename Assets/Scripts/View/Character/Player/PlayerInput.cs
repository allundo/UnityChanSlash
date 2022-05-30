using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

/// <summary>
/// Convert player input UIs operation into a ICommand and enqueue it to PlayerCommander.<br />
/// In addition to MobInput, there is 'Trigger type' input implemented to improve usability.<br />
/// </summary>
[RequireComponent(typeof(PlayerCommandTarget))]
[RequireComponent(typeof(PlayerMapUtil))]
public class PlayerInput : ShieldInput
{
    // Fight UI to fight against Enemy on front Tile
    [SerializeField] protected FightCircle fightCircle = default;

    // Handling UIs to handle Door or Item on front Tile
    [SerializeField] protected DoorHandler doorHandler = default;
    [SerializeField] protected ItemHandler itemHandler = default;
    [SerializeField] protected BoxHandler boxHandler = default;
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
    [SerializeField] protected InspectUI inspectUI = default;

    [SerializeField] protected Button restButton = default;

    [SerializeField] protected GameObject uiMask = default;

    protected PlayerCommandTarget playerTarget;
    protected bool IsAttack => commander.currentCommand is PlayerAttack;
    protected bool IsDash => commander.currentCommand is PlayerDash;
    protected bool IsForward => commander.currentCommand is PlayerForward;
    protected bool IsMessage => commander.currentCommand is PlayerMessage;

    private IReactiveProperty<bool> isEnemyDetected = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsEnemyDetected => isEnemyDetected;

    /// <summary>
    /// Stops Trigger type input if false.
    /// Trigger type input is validated earlier than Normal type input<br />
    /// but invalidated while moving Commands are executed continuously.
    /// </summary>
    protected bool isTriggerValid = true;

    /// <summary>
    /// Visible flag of all input UIs.
    /// </summary>
    protected bool isInputVisible = false;

    // Reserved ICommand input applied by other classes.
    // FIXME: need to implement game events handling system.
    public ICommand EnqueueDropFloor() => Interrupt(new PlayerDropFloor(playerTarget, 220f));
    public ICommand EnqueueTurnL() => ForceEnqueue(new PlayerTurnL(playerTarget, 18f, 0.99f, 0.99f));
    public ICommand EnqueueTurnR() => ForceEnqueue(new PlayerTurnR(playerTarget, 18f, 0.99f, 0.99f));
    public ICommand EnqueueMessage(MessageData[] data, bool isUIVisibleOnCompleted = true) => ForceEnqueue(new PlayerMessage(playerTarget, data, isUIVisibleOnCompleted));
    public ICommand InterruptMessage(MessageData[] data) => Interrupt(new PlayerMessage(playerTarget, data));

    public IObservable<ICommand> ObserveComplete(ICommand cmd)
        => (commander as PlayerCommander).CommandComplete
            .First(completedCommand => completedCommand == cmd)
            .IgnoreElements();

    public float RemainingDuration => IsIdling ? 0f : commander.currentCommand.RemainingDuration;

    protected override void SetCommander()
    {
        playerTarget = target as PlayerCommandTarget;
        commander = new PlayerCommander(playerTarget);
    }

    protected ICommand wakeUp;

    protected override void SetCommands()
    {
        die = new PlayerDie(playerTarget, 288f);
        wakeUp = new PlayerWakeUp(playerTarget, 120f);
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

    protected override ICommand GetCommand() => null;

    /// <summary>
    /// Input a ICommand to PlayerCommander. <br />
    /// When Normal input is applied, Trigger input is invalidated.
    /// </summary>
    /// <param name="cmd">Command to input</param>
    public override ICommand InputCommand(ICommand cmd)
    {
        bool isTriggerValid = this.isTriggerValid;
        this.isTriggerValid = false; // Disable Trigger input UI

        if (!isTriggerValid || !isCommandValid || cmd == null) return null;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
        return cmd;
    }

    /// <summary>
    /// Input a ICommand to PlayerCommander while isTriggerValid flag allows.
    /// </summary>
    public void InputTrigger(ICommand cmd)
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

    public override void ClearAll(bool isQueOnly = false, bool isValidInput = false, int threshold = 100)
    {
        commander.ClearAll(isQueOnly, isValidInput, threshold);
        isTriggerValid = isCommandValid = isValidInput;
    }

    /// <summary>
    /// Switch visibilities of all input UIs.
    /// </summary>
    public void SetInputVisible(bool isVisible = true)
    {
        isInputVisible = isVisible;
        restButton.gameObject.SetActive(isVisible);
        itemInventory.SetEnable(isVisible);
        if (!isVisible) InactivateUIs();
    }

    public override ICommand InputDie()
    {
        SetInputVisible(false);
        return base.InputDie();
    }

    public override ICommand InputIced(float duration)
    {
        var current = commander.currentCommand;
        if (current is PlayerJump && current.RemainingTimeScale > 0.25f || current is PlayerRun)
        {
            ClearAll();
            ICommand iced = new PlayerIcedFall(playerTarget, duration, 30f);
            Interrupt(iced);
            commander.EnqueueCommand(wakeUp);
            return iced;
        }

        return base.InputIced(duration);
    }

    protected override ICommand GetIcedCommand(float duration)
        => new PlayerIcedCommand(playerTarget, duration);
    public override void OnIceCrash()
    {

#if UNITY_EDITOR
        ICommand cmd = commander.currentCommand;
        bool isCurrentlyIced = cmd is PlayerIcedCommand || cmd is PlayerIcedFall;
        if (!isCurrentlyIced)
        {
            Debug.Log("IcedCrash(): " + gameObject.name + " isn't iced!, Command: " + cmd, gameObject);
        }
#endif

        commander.Cancel();
        SetInputVisible();
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
            fightCircle.SetActive(IsFightValid || IsAttack, forwardTile.GetEnemyStatus());
            fightCircle.isForwardMovable = forwardTile.IsEnterable();
        }
        else
        {
            fightCircle.Inactivate();
        }

        isEnemyDetected.Value = fightCircle.isActive;

        bool isFaceToDoor = !IsDash && !fightCircle.isActive && forwardTile is Door && !forwardTile.IsItemOn;

        if (isFaceToDoor)
        {
            Door door = forwardTile as Door;

            doorHandler.SetActive(door.IsControllable, door.IsOpen);
            forwardUI.Resize(0.5f, 1f);
            handleIcon.isLocked = door.IsLocked;
        }
        else
        {
            doorHandler.Inactivate();
        }

        bool isFaceToBox = !IsDash && !fightCircle.isActive && forwardTile is Box;
        if (isFaceToBox)
        {
            Box box = forwardTile as Box;
            boxHandler.SetActive(box.IsControllable, box.IsOpen);
            handleIcon.isLocked = box.IsLocked;
        }
        else
        {
            boxHandler.Inactivate();
        }

        bool isFaceToItem = !IsDash && !fightCircle.isActive && !isFaceToBox && forwardTile.IsItemOn;
        if (isFaceToItem)
        {
            itemHandler.Activate(forwardTile.TopItem);
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            itemHandler.Inactivate();
        }

        bool isHandleIconOn = isFaceToDoor || isFaceToBox || isFaceToItem || itemInventory.IsPutItem;
        if (!isHandleIconOn)
        {
            forwardUI.Resize(1f, 1f);
            handleIcon.Disable();
        }

        if (forwardTile is MessageWall)
        {
            inspectUI.SetActive(forwardTile as MessageWall, mobMap.dir, fightCircle.isActive);
        }
        else
        {
            inspectUI.Inactivate();
        }

        bool isHandleUIOn = doorHandler.isPressed || boxHandler.isPressed || itemHandler.isPressed || itemInventory.IsPutItem;

        uiMask.SetActive(isHandleUIOn || IsAttack || fightCircle.IsPressed);

        forwardUI.SetActive(forwardTile.IsEnterable(map.dir) && !isHandleUIOn);
        backwardUI.SetActive(mobMap.IsBackwardMovable);
        rightUI.SetActive(mobMap.IsRightMovable);
        leftUI.SetActive(mobMap.IsLeftMovable);

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
        boxHandler.Inactivate();
        itemHandler.Inactivate();
        fightCircle.Inactivate();

        inspectUI.Inactivate();

        forwardUI.Inactivate();
        backwardUI.Inactivate();
        rightUI.Inactivate();
        leftUI.Inactivate();

        turnRUI.Inactivate();
        turnLUI.Inactivate();
        jumpUI.Inactivate();

        guardUI.Inactivate();

        handleIcon.Disable();
    }

    /// <summary>
    /// Reserves fight behavior Commands for each attack button
    /// </summary>
    protected void InitFightInput()
    {
        // TODO: Refer duration and cancel time value from AttackButton object
        ICommand jab = new PlayerJab(playerTarget, 21.6f);
        ICommand straight = new PlayerStraight(playerTarget, 30f);
        ICommand kick = new PlayerKick(playerTarget, 43f);

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
        ICommand forward = new PlayerForward(playerTarget, 36f);
        ICommand handle = new PlayerHandle(playerTarget, 14.4f);
        ICommand getItem = new PlayerGetItem(playerTarget, 28.8f);
        PlayerPutItem putItem = new PlayerPutItem(playerTarget, 14.4f);
        PlayerHandleBox handleBox = new PlayerHandleBox(playerTarget, 14.4f);

        Observable.Merge(doorHandler.ObserveGo, itemHandler.ObserveGo)
            .Subscribe(_ => InputCommand(forward))
            .AddTo(this);

        PlayerAnimator anim = playerTarget.anim as PlayerAnimator;
        Observable.Merge(doorHandler.ObserveHandOn, boxHandler.ObserveHandOn, itemHandler.ObserveHandOn, itemInventory.OnPutItem.Select(itemIcon => itemIcon != null))
            .Subscribe(isHandOn => anim.handOn.Bool = isHandOn)
            .AddTo(this);

        doorHandler.ObserveHandle
            .Subscribe(_ => InputTrigger(handle))
            .AddTo(this);

        boxHandler.ObserveHandle
            .Subscribe(_ => InputTrigger(handleBox))
            .AddTo(this);

        itemHandler.ObserveGet
            .Subscribe(_ => InputTrigger(getItem))
            .AddTo(this);

        itemHandler.ObserveInspect
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerMessage(playerTarget, data)))
            .AddTo(this);

        itemInventory.OnPutApply
            .Subscribe(itemIcon => InputTrigger(putItem.SetItem(itemIcon)))
            .AddTo(this);

        itemInventory.OnUseItem
            .Subscribe(itemInfo => Interrupt(new PlayerItem(playerTarget, itemInfo), false))
            .AddTo(this);

        itemInventory.OnInspectItem
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerMessage(playerTarget, data)))
            .AddTo(this);

        inspectUI.OnInspectMessage
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerMessage(playerTarget, data)))
            .AddTo(this);
    }

    /// <summary>
    /// Reserves move and action Commands for each move and action buttons.
    /// </summary>
    protected void InitMoveInput()
    {
        ICommand forward = new PlayerForward(playerTarget, 36f);
        ICommand right = new PlayerRight(playerTarget, 43f);
        ICommand left = new PlayerLeft(playerTarget, 43f);
        ICommand backward = new PlayerBack(playerTarget, 43f);
        ICommand run = new PlayerRun(playerTarget, 24f);
        ICommand startRunning = new PlayerStartRunning(playerTarget, 24f);
        var brake = new PlayerBrakeStop(playerTarget, 48f);

        ICommand turnR = new PlayerTurnR(playerTarget, 18f);
        ICommand turnL = new PlayerTurnL(playerTarget, 18f);
        ICommand jump = new PlayerJump(playerTarget, 72f);

        ICommand guard = new GuardCommand(playerTarget, 1.44f);

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

    public void InputStop()
    {
        if (commander.currentCommand is PlayerRun)
        {
            Interrupt(new PlayerBrakeStop(playerTarget, 48f), false, true);
        }
        else
        {
            commander.ClearAll(true);
        }
    }

    public class PlayerGuardState : GuardState
    {
        private PlayerInput playerInput;

        private IObservable<bool> IsAutoGuardObservable => playerInput.IsEnemyDetected.SkipLatestValueOnSubscribe();
        private bool isAutoGuard => playerInput.IsEnemyDetected.Value;

        public override bool IsShieldOn(IDirection attackDir)
            => input.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

        public PlayerGuardState(PlayerInput input, float duration = 15f, float timeToReady = 0.15f) : base(input, duration, timeToReady)
        { }

        protected override void Subscribe(ShieldInput input)
        {
            playerInput = input as PlayerInput;

            Observable.Merge(IsAutoGuardObservable, IsShieldObservable)
                .Select(_ => isAutoGuard || playerInput.IsShield)
                .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
                .AddTo(playerInput.gameObject);
        }

        protected override void SetShieldReady(bool isGuardOn)
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
    }
}
