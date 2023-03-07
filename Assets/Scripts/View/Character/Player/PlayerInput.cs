using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public interface IPlayerInput : IInput
{
    void SetInputVisible(bool isVisible = true, bool withSubUI = true);
    void SetSubUIEnable(bool isEnable = true);
    void SetCancel();
}

/// <summary>
/// Convert player input UIs operation into a ICommand and enqueue it to PlayerCommander.<br />
/// In addition to MobInput, there is 'Trigger type' input implemented to improve usability.<br />
/// </summary>
[RequireComponent(typeof(PlayerCommandTarget))]
[RequireComponent(typeof(PlayerMapUtil))]
public class PlayerInput : ShieldInput, IPlayerInput
{
    // Fight UI to fight against Enemy on front Tile
    [SerializeField] protected FightCircle fightCircle = default;
    [SerializeField] protected AttackInputController attackInputUI = default;
    private IDisposable attackButtonsDisposable = null;

    // Handling UIs to handle Door or Item on front Tile
    [SerializeField] protected DoorHandler doorHandler = default;
    [SerializeField] protected ItemHandler itemHandler = default;
    [SerializeField] protected InspectHandler inspectHandler = default;
    [SerializeField] protected BoxHandler boxHandler = default;
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
    [SerializeField] protected PointerEnterExitUI guardUI = default;
    [SerializeField] protected InspectUI inspectUI = default;

    [SerializeField] protected Button restButton = default;

    [SerializeField] protected GameObject uiMask = default;

    protected IPlayerMapUtil playerMap;
    protected PlayerCommandTarget playerTarget;
    protected ItemInventory itemInventory;

    protected bool IsAttack => currentCommand is PlayerAttackCommand;
    protected bool IsItemUse => currentCommand is PlayerItem || IsFiring && !(currentCommand as PlayerFire).isCancelable;
    protected bool IsFiring => currentCommand is PlayerFire;
    protected bool IsDash => currentCommand is PlayerDash;
    protected bool IsForward => currentCommand is PlayerForward;
    protected bool IsMessage => currentCommand is PlayerMessage;
    protected bool isGuardOn => guardUI.IsPressed.Value;

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
    public ICommand EnqueueTurnL() => ForceEnqueue(new PlayerTurnL(playerTarget, 18f, 0.99f, 0.99f));
    public ICommand EnqueueTurnR() => ForceEnqueue(new PlayerTurnR(playerTarget, 18f, 0.99f, 0.99f));
    public ICommand EnqueueLanding(Vector3 moveVec) => ForceEnqueue(new PlayerLanding(playerTarget, 36f, moveVec));
    public ICommand InterruptIcedFall(float framesToMelt, bool isPitFall = false)
    {
        ICommand cmd = Interrupt(new PlayerIcedFall(playerTarget, isPitFall ? framesToMelt + 30f : framesToMelt, 30f));
        ForceEnqueue(wakeUp);
        return cmd;
    }
    public ICommand EnqueueMessage(MessageData[] data, bool isUIVisibleOnCompleted = true) => ForceEnqueue(new PlayerMessage(playerTarget, data, isUIVisibleOnCompleted));
    public ICommand InterruptMessage(MessageData[] data) => Interrupt(new PlayerMessage(playerTarget, data));

    public IObservable<ICommand> ObserveComplete(ICommand cmd)
        => (commander as PlayerCommander).CommandComplete
            .First(completedCommand => completedCommand == cmd);

    public bool HasNextCommand => commander.NextCommand != null;

    protected override void Awake()
    {
        base.Awake();
        playerMap = map as IPlayerMapUtil;
        itemInventory = ItemInventory.Instance;
    }

    protected override void SetCommander()
    {
        playerTarget = target as PlayerCommandTarget;
        commander = new PlayerCommander(playerTarget);
    }

    protected ICommand wakeUp;

    protected override void SetCommands()
    {
        die = new PlayerDie(playerTarget, 288f);
        wakeUp = new PlayerWakeUp(playerTarget, 150f);
    }

    protected override void SetInputs()
    {
        guardState = new PlayerGuardState(this);

        InitFightInput();
        InitHandleInput();
        InitMoveInput();
    }

    public void SetFightInput(IEquipmentStyle equipments)
    {
        attackButtonsDisposable?.Dispose();

        var attackButtonsHandler = attackInputUI.SetAttackButtonsHandler(equipments);
        var inputRegion = fightCircle.SetInputRegion(equipments);

        attackButtonsHandler.SetCommands(playerTarget);
        attackButtonsHandler.SetRegions(inputRegion);

        attackButtonsDisposable = attackButtonsHandler.AttackButtons()
            .Subscribe(cmd => InputCommand(cmd))
            .AddTo(this);

        // Disable EquipInventory during fight.
        IsEnemyDetected
            .Subscribe(isDetected => itemInventory.SetEquipEnable(!isDetected))
            .AddTo(this);
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

        bool isCommandEmpty = cmd == null;

        if (!BrakeIfRunning(!isCommandEmpty) && (!isTriggerValid || !isCommandValid || isCommandEmpty)) return null;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
        return cmd;
    }

    /// <summary>
    /// Input a ICommand to PlayerCommander while isTriggerValid flag allows.
    /// </summary>
    public void InputTrigger(ICommand cmd)
    {
        bool isCommandEmpty = cmd == null;
        if (!BrakeIfRunning(!isCommandEmpty) && (!isTriggerValid || isCommandEmpty)) return;

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
    public void SetInputVisible(bool isVisible = true, bool withSubUIs = true)
    {
        isInputVisible = isVisible;

        if (withSubUIs) SetSubUIEnable(isVisible);

        if (!isVisible) InactivateUIs();
    }

    public void SetSubUIEnable(bool isEnable = true)
    {
        restButton.gameObject.SetActive(isEnable);
        itemInventory.SetEnable(isEnable);
    }

    public void SetCancel() => (commander as PlayerCommander).SetCancel();

    public override ICommand InterruptDie()
    {
        SetInputVisible(false);
        return base.InterruptDie();
    }

    public override ICommand InputIced(float duration)
    {
        var current = commander.currentCommand;
        if (current is PlayerJump && current.RemainingTimeScale > 0.25f || current is PlayerRun)
        {
            ClearAll();
            ICommand iced = InterruptIcedFall(duration);
            return iced;
        }

        return base.InputIced(duration);
    }

    public ICommand InputPitFall(float damage)
    {
        ICommand pitFall = new PlayerPitFall(playerTarget, damage, 40f);
        Interrupt(pitFall, true, true);
        commander.EnqueueCommand(wakeUp);
        return pitFall;
    }

    protected override ICommand GetIcedCommand(float duration, float validateTiming)
        => new PlayerIcedCommand(playerTarget, duration, validateTiming);

    public override void OnIceCrash()
    {
        base.OnIceCrash();
        SetInputVisible();
    }

    /// <summary>
    /// Switches display of input UIs according to the situation for every frames.
    /// </summary>
    private void DisplayInputUIs()
    {
        if (!isInputVisible) return;

        ITile forwardTile = map.ForwardTile;

        bool isFaceToEnemy = !playerMap.isInPit && forwardTile.IsEnemyOn;
        if (isFaceToEnemy)
        {
            fightCircle.SetActive(IsFightValid || IsAttack || IsItemUse || IsFiring, forwardTile.GetEnemyStatus());
            fightCircle.isForwardMovable = forwardTile.IsEnterable();
        }
        else
        {
            fightCircle.Inactivate();
        }

        isEnemyDetected.Value = fightCircle.isActive;

        bool isFaceToDoor = !playerMap.isInPit && !IsDash && !fightCircle.isActive && forwardTile is Door && !forwardTile.IsItemOn;

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

        bool isFaceToBox = !playerMap.isInPit && !IsDash && !fightCircle.isActive && forwardTile is Box;
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

        bool isFaceToItem = !playerMap.isInPit && !IsDash && !fightCircle.isActive && !isFaceToBox && forwardTile.IsItemOn;
        if (isFaceToItem)
        {
            itemHandler.Activate(forwardTile.TopItem);
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            itemHandler.Inactivate();
        }

        bool isFaceToSpecialTile = !playerMap.isInPit && IsIdling && !fightCircle.isActive && forwardTile is Pit;
        if (isFaceToSpecialTile)
        {
            inspectHandler.Activate(forwardTile);
            forwardUI.Resize(0.5f, 1f);
        }
        else
        {
            inspectHandler.Inactivate();
        }

        bool isHandleIconOn = isFaceToDoor || isFaceToBox || isFaceToItem || isFaceToSpecialTile || itemInventory.IsPutItem;
        if (!isHandleIconOn)
        {
            forwardUI.Resize(1f, 1f);
            handleIcon.Disable();
        }

        // Is face to Message Board
        if (forwardTile is MessageWall)
        {
            inspectUI.SetActive(forwardTile as MessageWall, mobMap.dir, fightCircle.isActive);
        }
        else
        {
            inspectUI.Inactivate();
        }

        bool isHandleUIOn = doorHandler.isPressed || boxHandler.isPressed || itemHandler.isPressed || inspectHandler.isPressed || itemInventory.IsPutItem;

        uiMask.SetActive(isHandleUIOn || IsAttack || attackInputUI.IsPressed);

        forwardUI.SetActive(forwardTile.IsEnterable(map.dir) && !playerMap.isInPit && !isHandleUIOn);
        backwardUI.SetActive(playerMap.IsBackwardMovable);
        rightUI.SetActive(playerMap.IsRightMovable);
        leftUI.SetActive(playerMap.IsLeftMovable);

        bool isTriggerActive = fightCircle.isActive || isTriggerValid || isCommandValid || IsShield || IsDash;

        forwardUI.SetDashInputActive(IsForward || isTriggerActive);
        turnRUI.SetActive(isTriggerActive, fightCircle.isActive);
        turnLUI.SetActive(isTriggerActive, fightCircle.isActive);
        jumpUI.SetActive(isTriggerActive, fightCircle.isActive);

        guardUI.SetActive(!fightCircle.isActive && !playerMap.isInPit);
    }

    /// <summary>
    /// Inactivate all UIs
    /// </summary>
    private void InactivateUIs()
    {
        doorHandler.Inactivate();
        boxHandler.Inactivate();
        itemHandler.Inactivate();
        fightCircle.Inactivate(true);

        inspectUI.Inactivate();
        inspectHandler.Inactivate();

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
    /// Detect player's charging up an attack
    /// </summary>
    protected void InitFightInput()
    {
        PlayerAnimator anim = playerTarget.anim as PlayerAnimator;
        attackInputUI.IsChargingUp
            .Subscribe(isChargingUp => anim.chargeUp.Bool = isChargingUp)
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

        Observable.Merge(doorHandler.ObserveGo, itemHandler.ObserveGo, inspectHandler.ObserveGo)
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
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerInfoMessage(playerTarget, data)))
            .AddTo(this);

        inspectHandler.ObserveInspect
            .Subscribe(tile => InputTrigger(IsMessage ? null : new PlayerInspectTile(playerTarget, tile)))
            .AddTo(this);

        itemInventory.OnPutApply
            .Subscribe(itemIcon => InputTrigger(putItem.SetItem(itemIcon)))
            .AddTo(this);

        itemInventory.OnUseItem
            .Subscribe(itemInfo => { if (!IsItemUse) Interrupt(new PlayerItem(playerTarget, itemInfo), IsFiring); }) // Cancel current PlayerFire command if cancelable.
            .AddTo(this);

        itemInventory.OnInspectItem
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerInfoMessage(playerTarget, data)))
            .AddTo(this);

        inspectUI.OnInspectMessage
            .Subscribe(data => InputTrigger(IsMessage ? null : new PlayerInspectWall(playerTarget, data)))
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
        ICommand run = new PlayerRun(playerTarget);
        ICommand startRunning = new PlayerStartRunning(playerTarget);

        ICommand turnR = new PlayerTurnR(playerTarget, 18f);
        ICommand turnL = new PlayerTurnL(playerTarget, 18f);
        ICommand jump = new PlayerJump(playerTarget, 72f);
        ICommand pitJump = new PlayerPitJump(playerTarget, 60f);

        forwardUI.EnterObservable
            .Subscribe(_ => InputCommand(forward))
            .AddTo(this);

        forwardUI.DashObservable
            .Subscribe(_ =>
            {
                if (commander.currentCommand is PlayerForward)
                {
                    // Cancel forward command and start dash
                    Interrupt(startRunning);
                    return;
                }

                // Reserve dash on command queue
                InputTrigger(run);
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
            .Subscribe(_ =>
            {
                if (commander.NextCommand is PlayerDash)
                {
                    commander.ReplaceNext(jump);
                    return;
                }
                InputTrigger(playerMap.isInPit ? pitJump : jump);
            })
            .AddTo(this);

        guardUI.IsPressed
            .Subscribe(isPressed =>
            {
                BrakeIfRunning(false); // Guard must be overwritten by next command
                (commander as PlayerCommander).SetGuard(isPressed);
            })
            .AddTo(this);
    }

    public void InputStop()
    {
        if (!BrakeIfRunning()) commander.ClearAll(true);
    }

    private bool BrakeIfRunning(bool isCommandInput = false)
    {
        if (commander.currentCommand is PlayerBrake && isCommandInput)
        {
            // Overwrite next command during braking.
            commander.ClearAll(true, true);
            return true;
        }

        bool isRunning = commander.currentCommand is PlayerRun && !(commander.NextCommand is PlayerBrake);

        if (isRunning)
        {
            float remaining = commander.currentCommand.RemainingTimeScale;
            bool isHalf = remaining > 0.333333f;
            ICommand brake = isHalf ? new PlayerBrakeStopHalf(playerTarget, remaining, isCommandInput) : new PlayerBrakeStop(playerTarget, isCommandInput);

            Interrupt(brake, isHalf, true);
        }
        return isRunning;
    }

    public class PlayerGuardState : GuardState
    {
        private PlayerInput playerInput;

        private IObservable<bool> IsAutoGuardObservable => playerInput.IsEnemyDetected.SkipLatestValueOnSubscribe();
        private bool isAutoGuard => playerInput.IsEnemyDetected.Value;

        public override bool IsShieldOn(IDirection attackDir)
            => playerInput.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

        public override float SetShield()
        {
            // Don't play shield motion when charging up.
            if ((anim as PlayerAnimator).chargeUp.Bool) return 0.5f;

            input.ClearAll(true, false, 9);
            input.Interrupt(shieldOn);
            if (playerInput.isGuardOn) (input.commander as PlayerCommander).SetGuard(true);
            playerInput.itemInventory.UseShield();
            return 1f;
        }

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
    }
}
