using UnityEngine;
using System;
using UniRx;
using DG.Tweening;

public abstract class PlayerCommand : MobCommand
{
    protected PlayerCommandTarget playerTarget;
    protected PlayerAnimator playerAnim;
    protected PlayerInput playerInput;
    protected ThirdPersonCamera mainCamera;
    protected HidePlateHandler hidePlateHandler;
    protected ItemGenerator itemGenerator;
    protected ItemInventory itemInventory;
    protected MessageController messageController;

    protected float triggerInvalidDuration;

    public PlayerCommand(PlayerCommandTarget target, float duration, float validateTiming = 0.5f, float triggerTiming = 0.5f)
        : base(target, duration, validateTiming)
    {
        playerTarget = target;
        this.triggerInvalidDuration = this.duration * triggerTiming;

        playerAnim = anim as PlayerAnimator;
        playerInput = input as PlayerInput;
        mainCamera = target.mainCamera;
        hidePlateHandler = target.hidePlateHandler;
        itemGenerator = target.itemGenerator;
        itemInventory = playerInput.GetItemInventory;
        messageController = target.messageController;
    }

    protected override Tween ValidateTween()
    {
        Tween validateTween = base.ValidateTween();
        if (triggerInvalidDuration >= invalidDuration) return validateTween;

        return DOTween.Sequence()
            .Join(DOTweenTimer(triggerInvalidDuration, () => input.ValidateInput(true)))
            .Join(validateTween);
    }

    protected void SetUIInvisible(bool isVisibleOnCompleted = true)
    {
        playerInput.SetInputVisible(false);
        onCompleted.Add(() => playerInput.SetInputVisible(isVisibleOnCompleted));
    }
}

public abstract class PlayerMove : PlayerCommand
{
    protected ITile destTile;
    public PlayerMove(PlayerCommandTarget target, float duration) : base(target, duration, 0.95f, 0.5f) { }

    protected abstract bool IsMovable { get; }
    protected abstract Pos GetDest { get; }
    protected abstract ITile DestTile { get; }

    protected void SetSpeed()
    {
        playerAnim.speed.Float = Speed;
        playerAnim.rSpeed.Float = RSpeed;
    }

    protected void ResetSpeed()
    {
        playerAnim.speed.Float = 0.0f;
        playerAnim.rSpeed.Float = 0.0f;
    }

    protected override bool Action()
    {
        if (!IsMovable)
        {
            return false;
        }

        playingTween = tweenMove.Linear(GetDest);

        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).OnComplete(hidePlateHandler.Move).Play();

        return true;
    }
}

public class PlayerForward : PlayerMove
{
    public PlayerForward(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsForwardMovable;
    protected override ITile DestTile => mobMap.ForwardTile;
    protected override Pos GetDest => mobMap.GetForward;
    protected override float Speed => TILE_UNIT / duration;
}

public class PlayerBack : PlayerMove
{
    public PlayerBack(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsBackwardMovable;
    protected override ITile DestTile => mobMap.BackwardTile;
    protected override Pos GetDest => mobMap.GetBackward;
    protected override float Speed => -TILE_UNIT / duration;
}

public class PlayerRight : PlayerMove
{
    public PlayerRight(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsRightMovable;
    protected override ITile DestTile => mobMap.RightTile;
    protected override Pos GetDest => mobMap.GetRight;
    protected override float RSpeed => TILE_UNIT / duration;
}

public class PlayerLeft : PlayerMove
{
    public PlayerLeft(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsLeftMovable;
    protected override ITile DestTile => mobMap.LeftTile;
    protected override Pos GetDest => mobMap.GetLeft;
    protected override float RSpeed => -TILE_UNIT / duration;
}

public class PlayerDash : PlayerCommand
{
    public override int priority => 10;
    public PlayerDash(PlayerCommandTarget target, float duration) : base(target, duration, 0.95f)
    { }

    protected Tween ToSpeed(float endValue, float timeScale = 1f)
        => DOTween.To(
            () => playerAnim.speed.Float,
            value => playerAnim.speed.Float = value,
            endValue,
            duration * timeScale
        );
}

public class PlayerStartRunning : PlayerRun
{
    protected Command run;
    public PlayerStartRunning(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        run = new PlayerRun(target, duration);
    }

    public override IObservable<Unit> Execute()
    {
        var dest = mobMap.DestVec;                     // Remaining vector to front tile
        var timeScale = dest.magnitude / TILE_UNIT; // Remaining distance rate

        playingTween = tweenMove.Linear(mobMap.CurrentVec3Pos + dest, timeScale);

        playerAnim.speed.Float = TILE_UNIT / duration * 0.5f;
        completeTween = DOTween.Sequence()
            .Append(ToSpeed(TILE_UNIT / duration, 0.25f))
            .InsertCallback(duration * timeScale, () =>
            {
                playerAnim.speed.Float = 0f;
                hidePlateHandler.Move();
            })
            .Play();

        target.input.Interrupt(run, false);

        return ObservableComplete(timeScale);
    }
}

public class PlayerRun : PlayerDash
{
    protected ICommand brakeAndBackStep;
    public PlayerRun(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        brakeAndBackStep = new PlayerBrakeAndBackStep(target, duration * 2f);
    }

    public override IObservable<Unit> Execute()
    {
        if (mobMap.IsForwardMovable)
        {
            playingTween = tweenMove.Linear(mobMap.GetForward, 1f);

            playerAnim.speed.Float = TILE_UNIT / duration;
            completeTween = tweenMove
                .FinallyCall(() => playerAnim.speed.Float = 0)
                .OnComplete(hidePlateHandler.Move)
                .Play();

            target.input.Interrupt(this, false);

            return ObservableComplete();
        }
        else
        {
            input.Interrupt(brakeAndBackStep, false);
            return Observable.Empty<Unit>();
        }
    }
}

public class PlayerBrake : PlayerDash
{
    public PlayerBrake(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected Tween DampenSpeed(TweenCallback startCallback = null, float timeScale = 1f, float delayRate = 0f)
    {
        playerAnim.speed.Float = TILE_UNIT / duration * 2f;

        var seq = DOTween.Sequence();

        if (delayRate > 0f) seq.AppendInterval(duration * delayRate);

        if (startCallback != null) seq.AppendCallback(startCallback);

        return seq.Append(ToSpeed(0f, timeScale)).OnComplete(hidePlateHandler.Move).Play();
    }
}

public class PlayerBrakeStop : PlayerBrake
{
    protected ICommand brakeAndBackStep;
    public PlayerBrakeStop(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        brakeAndBackStep = new PlayerBrakeAndBackStep(target, duration);
    }

    public override IObservable<Unit> Execute()
    {

        if (mobMap.IsForwardMovable)
        {
            playingTween = tweenMove.Brake(mobMap.GetForward, 0.75f);
            validateTween = ValidateTween().Play();
            completeTween = DampenSpeed(playerAnim.brake.Fire, 0.5f, 0.3f);

            return ObservableComplete();
        }
        else
        {
            input.Interrupt(brakeAndBackStep, false);
            return Observable.Empty<Unit>();
        }
    }
}

public class PlayerBrakeAndBackStep : PlayerBrake
{
    protected float startSpeedRate;
    public PlayerBrakeAndBackStep(PlayerCommandTarget target, float duration, float startSpeedRate = 2f) : base(target, duration)
    {
        this.startSpeedRate = startSpeedRate;
    }

    public override IObservable<Unit> Execute()
    {
        playingTween = tweenMove.BrakeAndBack();
        validateTween = ValidateTween().Play();
        completeTween = DampenSpeed(playerAnim.brakeAndBackStep.Fire);

        return ObservableComplete();
    }
}

public class PlayerJump : PlayerCommand
{
    public PlayerJump(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playerAnim.jump.Fire();

        playingTween = tweenMove
            .JumpLeap(
                mobMap.IsJumpable ? 2 : mobMap.IsForwardMovable ? 1 : 0,
                hidePlateHandler.Move,  // Update HidePlate on entering the leaped Tile
                hidePlateHandler.Move   // Update HidePlate on entering the destination Tile
            );

        return true;
    }
}
public class PlayerLanding : PlayerCommand
{
    public PlayerLanding(PlayerCommandTarget target, float duration, Vector3 moveVec) : base(target, duration)
    {
        playingTween = tweenMove.Landing(moveVec);
    }

    public PlayerLanding(PlayerCommandTarget target, float duration, float moveForward = 0.5f, float moveY = 0.75f) : base(target, duration)
    {
        Vector3 moveVec = map.dir.LookAt * moveForward + Vector3.up * moveY;
        playingTween = tweenMove.Landing(moveVec);
    }

    protected override bool Action()
    {
        Debug.Log("PlayerLanding Action");
        playerAnim.landing.Fire();
        playingTween.Play();

        return true;
    }
}

public class PlayerPitJump : PlayerCommand
{
    public PlayerPitJump(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        Vector3 moveVec;
        float jumpPower = 0.1f;

        if ((map as IPlayerMapUtil).IsPitJumpable)
        {
            map.MoveObjectOn(map.GetForward);
            moveVec = map.DestVec;
            completeTween = tweenMove.DelayedCall(0.8f, hidePlateHandler.Move).Play();
            playerInput.SetInputVisible();
        }
        else
        {
            moveVec = Vector3.zero;
            jumpPower = 1.2f;
        }

        playerAnim.pitJump.Fire();
        playingTween = tweenMove.JumpRelative(moveVec, 0.8f, jumpPower).SetEase(Ease.OutQuad).Play();

        return true;
    }
}

public class PlayerIcedFall : PlayerCommand
{
    public override int priority => 20;
    protected float meltFrameTimer;
    public PlayerIcedFall(PlayerCommandTarget target, float framesToMelt, float duration) : base(target, duration)
    {
        meltFrameTimer = Mathf.Min(framesToMelt, duration + 1f) * FRAME_UNIT;
    }

    public override IObservable<Unit> Execute()
    {
        playerAnim.speed.Float = 0f;
        playerAnim.fall.Bool = true;

        playerInput.SetInputVisible(false);

        Vector3 moveVec = mobMap.DestVec;

        var jumpSeq = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.JumpRelative(moveVec, 1f, 0f).SetEase(Ease.Linear))
            .InsertCallback(0.95f * duration, hidePlateHandler.Move) // Update HidePlate on entering the destination Tile
            .AppendCallback(() => mobReact.Damage(0.5f, null, AttackType.Smash));

        // Update HidePlate on entering the leaped Tile
        if (moveVec.magnitude / TILE_UNIT > 1f)
        {
            jumpSeq.InsertCallback(0.5f * duration, hidePlateHandler.Move);
        }

        playingTween = jumpSeq.SetUpdate(false).Play();

        completeTween = DOVirtual.DelayedCall(meltFrameTimer, () => mobReact.Melt(), false).Play();

        return ObservableComplete();
    }
}

public class PlayerPitFall : PlayerCommand
{
    public override int priority => 20;
    protected float damage;
    public PlayerPitFall(PlayerCommandTarget target, float damage, float duration) : base(target, duration)
    {
        this.damage = damage;
    }

    public override IObservable<Unit> Execute()
    {
        playerAnim.speed.Float = 0f;
        playerAnim.fall.Bool = true;

        playerInput.SetInputVisible(false);

        playingTween = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.Jump(mobMap.DestVec3Pos - new Vector3(0, TILE_UNIT, 0), 1f, 0.001f).SetEase(Ease.OutQuad))
            .AppendCallback(() => mobReact.Damage(new Attacker(damage, null, "落とし穴"), new Attack.AttackData(1f /* 'parameterless struct constructors' is not available in C# 9.0. */)))
            .AppendCallback(hidePlateHandler.Move)
            .SetUpdate(false)
            .Play();

        return ObservableComplete();
    }
}

public class PlayerWakeUp : PlayerCommand
{
    public override int priority => 20;
    protected float wakeUpTiming;
    public PlayerWakeUp(PlayerCommandTarget target, float duration, float wakeUpTiming = 0.5f) : base(target, duration)
    {
        this.wakeUpTiming = wakeUpTiming;
    }

    protected override bool Action()
    {
        completeTween = DOTween.Sequence()
            .InsertCallback(duration * wakeUpTiming, () => playerAnim.fall.Bool = false)
            .InsertCallback(duration * (1f + wakeUpTiming) * 0.5f, mobReact.OnWakeUp)
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class PlayerTurnL : PlayerCommand
{
    public PlayerTurnL(PlayerCommandTarget target, float duration, float validateTiming = 0.5f, float triggerTiming = 0.1f)
        : base(target, duration, validateTiming, triggerTiming) { }

    protected override bool Action()
    {
        mobMap.TurnLeft();
        mainCamera.TurnLeft();
        playerAnim.turnL.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(mobMap.dir);

        playingTween = tweenMove.TurnToDir.SetEase(Ease.InCubic).OnComplete(mainCamera.ResetCamera).Play();
        return true;
    }
}

public class PlayerTurnR : PlayerCommand
{
    public PlayerTurnR(PlayerCommandTarget target, float duration, float validateTiming = 0.5f, float triggerTiming = 0.1f)
        : base(target, duration, validateTiming, triggerTiming) { }

    protected override bool Action()
    {
        mobMap.TurnRight();
        mainCamera.TurnRight();
        playerAnim.turnR.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(mobMap.dir);

        playingTween = tweenMove.TurnToDir.SetEase(Ease.InCubic).OnComplete(mainCamera.ResetCamera).Play();
        return true;
    }
}

public abstract class PlayerAction : PlayerCommand
{
    public PlayerAction(PlayerCommandTarget target, float duration, float validateTiming = 0.5f)
        : base(target, duration, validateTiming, validateTiming * 0.5f) { }
}

public class PlayerHandle : PlayerAction
{
    public PlayerHandle(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (playerAnim.handOn.Bool)
        {
            playerAnim.handle.Fire();
            return true;
        }

        return false;
    }
}

public class PlayerGetItem : PlayerAction
{
    public PlayerGetItem(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        ITile tile = mobMap.ForwardTile;
        Item item = tile.PickItem();

        if (playerAnim.handOn.Bool && itemInventory.PickUp(item.itemInfo))
        {
            ActiveMessageController.Instance.InputMessageData(ActiveMessageData.GetItem(item.itemInfo));
            playerAnim.getItem.Fire();
        }
        else
        {
            tile.PutItem(item);
            return false;
        }
        return true;
    }
}

public class PlayerHandleBox : PlayerAction
{
    public PlayerHandleBox(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        Box boxTile = mobMap.ForwardTile as Box;

        // Cancel the command if forward tile isn't Box or Box isn't controllable or player isn't handling the Box.
        if (boxTile == null || !boxTile.IsControllable || !playerAnim.handOn.Bool) return false;

        bool isOpen = boxTile.IsOpen;

        boxTile.Handle();

        // Finish this handling command if player is closing the Box.
        if (isOpen) return true;

        Item item = boxTile.PickItem();

        // Cancel picking up an item if player failed to get item from the Box.
        if (item == null || !itemInventory.PickUp(item.itemInfo))
        {
            boxTile.PutItem(item);
            return false;
        }

        playerAnim.getItem.Fire();
        return true;
    }
}

public class PlayerPutItem : PlayerAction
{
    private ItemIcon itemIcon = null;
    public PlayerPutItem(PlayerCommandTarget target, float duration) : base(target, duration) { }

    public PlayerPutItem SetItem(ItemIcon itemIcon)
    {
        this.itemIcon = itemIcon;
        return this;
    }

    private bool HandOff(bool returnValue)
    {
        playerAnim.handOn.Bool = false;
        return returnValue;
    }

    protected override bool Action()
    {
        // Cancel if hand is not on yet.
        if (!playerAnim.handOn.Bool) return false;

        // Cancel if guarding
        if (playerAnim.guard.Bool) return HandOff(false);

        // Cancel if forward tile is Box and the Box isn't Open or Controllable.
        Box boxTile = mobMap.ForwardTile as Box;
        if (boxTile != null)
        {
            if (!boxTile.IsOpen)
            {
                ActiveMessageController.Instance.InputMessageData(new ActiveMessageData("箱の蓋が開いていない！", SDFaceID.SAD, SDEmotionID.CONFUSE));
                return HandOff(false);
            }

            if (!boxTile.IsControllable)
            {
                ActiveMessageController.Instance.InputMessageData(new ActiveMessageData("箱の蓋を閉じられない！", SDFaceID.SURPRISE, SDEmotionID.SURPRISE));
                return HandOff(false);
            }
        }

        // Cancel if putting item is invalid.
        if (!itemGenerator.Put(itemIcon.itemInfo, mobMap.GetForward, map.dir))
        {
            ActiveMessageController.Instance.InputMessageData(new ActiveMessageData("そこには捨てられない！", SDFaceID.EYECLOSE, SDEmotionID.CONFUSE));
            return HandOff(false);
        }

        if (boxTile != null) boxTile.Handle();

        itemInventory.Remove(itemIcon);
        playerAnim.putItem.Fire();
        itemIcon = null;

        return HandOff(true);
    }
}

public class PlayerAttack : PlayerAction
{
    protected IMobAttack attack;
    protected PlayerAnimator.AnimatorTrigger trigger;

    protected Tween cancelTimer = null;
    protected float cancelStart;

    protected PlayerAttack(PlayerCommandTarget target, float duration, float cancelStart = 1f) : base(target, duration, 0.04f)
    {
        this.cancelStart = cancelStart;

        if (cancelStart < 1f)
        {
            cancelTimer = DOTween.Sequence()
                .InsertCallback(cancelStart * duration * FRAME_UNIT, () => playerAnim.cancel.Bool = true)
                .InsertCallback((1 + cancelStart) * duration * FRAME_UNIT, () => playerAnim.cancel.Bool = false)
                .AsReusable(target.gameObject);
        }
    }

    protected override IObservable<Unit> ExecOnCompleted(params Action[] onCompleted)
    {
        onCompleted.ForEach(action => SetOnCompleted(action));
        return cancelStart < 1f ? ObservableCancelComplete() : ObservableComplete();
    }

    protected IObservable<Unit> ObservableCancelComplete()
    {
        return Observable.Create<Unit>(o =>
        {
            Tween onNext = DOTweenTimer(duration * cancelStart, null).OnComplete(() => o.OnNext(Unit.Default)).Play();
            Tween complete = DOTweenTimer(duration, DoOnCompleted).OnComplete(o.OnCompleted).Play();

            return Disposable.Create(() =>
            {
                onNext?.Kill();
                complete?.Kill();
            });
        });
    }

    protected override bool Action()
    {
        cancelTimer?.Restart();
        Attack();
        SetOnCompleted(ResetAnimatorParams);
        return true;
    }

    public override void Cancel()
    {
        ResetAnimatorParams();
        base.Cancel();
    }

    protected virtual void Attack()
    {
        trigger.Fire();
        completeTween = attack.AttackSequence(duration).Play();
    }

    protected virtual void ResetAnimatorParams() => trigger.Reset();
}

public class PlayerCriticalAttack : PlayerAttack
{
    protected PlayerCriticalAttack(PlayerCommandTarget target, float duration, float cancelStart = 1f) : base(target, duration, cancelStart) { }
    protected override void Attack()
    {
        playerAnim.critical.Bool = true;
        trigger.Fire();
        completeTween = attack.CriticalAttackSequence().Play();
    }

    protected override void ResetAnimatorParams()
    {
        trigger.Reset();
        playerAnim.critical.Bool = false;
    }
}

public class PlayerJab : PlayerAttack
{
    public PlayerJab(PlayerCommandTarget target, float duration) : base(target, duration, 0.6f)
    {
        attack = target.Attack(0) as IMobAttack;
        trigger = playerAnim.jab;
    }
}

public class PlayerJabCritical : PlayerCriticalAttack
{
    public PlayerJabCritical(PlayerCommandTarget target, float duration) : base(target, duration, 0.6f)
    {
        attack = target.Attack(0) as IMobAttack;
        trigger = playerAnim.jab;
    }
}

public class PlayerStraight : PlayerAttack
{
    public PlayerStraight(PlayerCommandTarget target, float duration) : base(target, duration, 0.8f)
    {
        attack = target.Attack(1) as IMobAttack;
        trigger = playerAnim.straight;
    }
}

public class PlayerStraightCritical : PlayerCriticalAttack
{
    public PlayerStraightCritical(PlayerCommandTarget target, float duration) : base(target, duration, 0.8f)
    {
        attack = target.Attack(1) as IMobAttack;
        trigger = playerAnim.straight;
    }
}

public class PlayerKick : PlayerAttack
{
    public PlayerKick(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        attack = target.Attack(2) as IMobAttack;
        trigger = playerAnim.kick;
    }
}

public class PlayerKickCritical : PlayerCriticalAttack
{
    public PlayerKickCritical(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        attack = target.Attack(2) as IMobAttack;
        trigger = playerAnim.kick;
    }
}

public class PlayerFire : PlayerAction
{
    protected BulletType type;
    public PlayerFire(PlayerCommandTarget target, float duration, BulletType type = BulletType.FireBall) : base(target, duration)
    {
        this.type = type;
    }

    protected override bool Action()
    {
        playerAnim.fire.Fire();
        completeTween = target.magic?.MagicSequence(type, duration * 2.5f)?.Play();
        return true;
    }
}
public class PlayerCoinThrow : PlayerAction
{
    public PlayerCoinThrow(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playerAnim.coin.Fire();
        completeTween = target.magic?.MagicSequence(BulletType.Coin, duration * 2.5f)?.Play();
        return true;
    }
}

public class PlayerItem : PlayerAction
{
    protected ItemInfo itemInfo;
    public override int priority => 5;

    public PlayerItem(PlayerCommandTarget target, ItemInfo itemInfo, float timing = 0.5f) : base(target, itemInfo.duration, timing)
    {
        this.itemInfo = itemInfo;
    }

    protected override bool Action()
    {
        playingTween = itemInfo.EffectSequence(playerTarget)?.Play();
        return true;
    }
}

public class PlayerDie : PlayerCommand
{
    public override int priority => 100;
    public PlayerDie(PlayerCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        itemInventory.Cancel();
        react.OnDie();
        playerAnim.die.Bool = true;
        playerTarget.gameOverUI.Play();

        DataStoreAgent.Instance.SaveDeadRecords((react as PlayerReactor).lastAttacker, GameManager.Instance.SumUpItemValue(), GameInfo.Instance.currentFloor);

        return ExecOnCompleted(() => mobReact.OnDisappear());
    }
}

public class PlayerDropFloor : PlayerCommand
{
    public PlayerDropFloor(PlayerCommandTarget target, float duration) : base(target, duration, 1f, 1f) { }

    protected override bool Action()
    {
        SetUIInvisible(false);
        playerAnim.dropFloor.Fire();
        playingTween = tweenMove.Drop(25.0f, 0f, 0.66f, 1.34f).Play();
        return true;
    }
}

public class PlayerMessage : PlayerAction
{
    protected MessageData[] data;
    protected bool isUIVisibleOnCompleted;

    public PlayerMessage(PlayerCommandTarget target, MessageData[] data, bool isUIVisibleOnCompleted = true) : base(target, 5f, 0.999f)
    {
        this.data = data;
        this.isUIVisibleOnCompleted = isUIVisibleOnCompleted;
    }

    protected override bool Action()
    {
        if (data == null) return false;
        SetUIInvisible(isUIVisibleOnCompleted);
        messageController.InputMessageData(data, isUIVisibleOnCompleted);
        return true;
    }
}

public class PlayerInspect : PlayerAction
{
    protected ActiveMessageData data;

    public PlayerInspect(PlayerCommandTarget target, ActiveMessageData data) : base(target, 2f, 0.999f)
    {
        this.data = data;
    }

    protected override bool Action()
    {
        ActiveMessageController.Instance.InputMessageData(data);
        return true;
    }
}

public class PlayerIcedCommand : PlayerCommand
{
    private float frames;
    public override int priority => 20;
    public PlayerIcedCommand(PlayerCommandTarget target, float duration) : base(target, duration, 0.98f)
    {
        frames = duration;
    }

    protected override bool Action()
    {
        anim.Pause();

        completeTween = tweenMove.DelayedCall(1f, anim.Resume).Play();
        SetUIInvisible();
        SetOnCompleted(() => mobReact.Melt());
        return true;
    }
}
