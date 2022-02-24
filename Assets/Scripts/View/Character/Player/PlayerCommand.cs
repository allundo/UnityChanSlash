using UnityEngine;
using System;
using UniRx;
using DG.Tweening;

public abstract class PlayerCommand : Command
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
        itemInventory = target.itemInventory;
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

    protected void SetUIInvisible()
    {
        playerInput.SetInputVisible(false);
        onCompleted.Add(() => playerInput.SetInputVisible(true));
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

        playingTween = tweenMove.Linear(GetDest).OnComplete(hidePlateHandler.Move).Play();

        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        return true;
    }
}

public class PlayerForward : PlayerMove
{
    public PlayerForward(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsForwardMovable;
    protected override ITile DestTile => map.ForwardTile;
    protected override Pos GetDest => map.GetForward;
    protected override float Speed => TILE_UNIT / duration;
}

public class PlayerBack : PlayerMove
{
    public PlayerBack(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsBackwardMovable;
    protected override ITile DestTile => map.BackwardTile;
    protected override Pos GetDest => map.GetBackward;
    protected override float Speed => -TILE_UNIT / duration;
}

public class PlayerRight : PlayerMove
{
    public PlayerRight(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsRightMovable;
    protected override ITile DestTile => map.RightTile;
    protected override Pos GetDest => map.GetRight;
    protected override float RSpeed => TILE_UNIT / duration;
}

public class PlayerLeft : PlayerMove
{
    public PlayerLeft(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsLeftMovable;
    protected override ITile DestTile => map.LeftTile;
    protected override Pos GetDest => map.GetLeft;
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
        var dest = map.DestVec;
        var timeScale = dest.magnitude / TILE_UNIT;

        playingTween = tweenMove.Linear(map.CurrentVec3Pos + dest, timeScale, hidePlateHandler.Move);

        playerAnim.speed.Float = TILE_UNIT / duration * 0.5f;
        completeTween = DOTween.Sequence()
            .Append(ToSpeed(TILE_UNIT / duration, 0.25f))
            .AppendInterval(duration * (timeScale - 0.25f))
            .AppendCallback(() => playerAnim.speed.Float = 0f)
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
        if (map.IsForwardMovable)
        {
            playingTween = tweenMove.Linear(map.GetForward, 1f, hidePlateHandler.Move);

            playerAnim.speed.Float = TILE_UNIT / duration;
            completeTween = tweenMove.FinallyCall(() => playerAnim.speed.Float = 0).Play();

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

        return seq.Append(ToSpeed(0f, timeScale)).Play();
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

        if (map.IsForwardMovable)
        {
            playingTween = tweenMove.Brake(map.GetForward, 0.75f, hidePlateHandler.Move);
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

    protected Pos prevPos = new Pos();

    protected override bool Action()
    {
        int distance = 0;
        ITile destTile = null;

        if (map.IsJumpable)
        {
            distance = 2;
            destTile = map.JumpTile;
        }
        else if (map.IsForwardMovable)
        {
            distance = 1;
            destTile = map.ForwardTile;
        }

        playerAnim.jump.Fire();

        playingTween = tweenMove
            .Jump(
                distance,
                hidePlateHandler.Move,  // Update HidePlate on entering the next Tile
                hidePlateHandler.Move   // Update HidePlate on entering the next next Tile
            );

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
        playerAnim.icedFall.Bool = true;

        playingTween = DOTween.Sequence()
            .AppendCallback(react.OnFall)
            .Append(tweenMove.Jump(map.DestVec3Pos, 1f, 0f).SetEase(Ease.Linear))
            .AppendCallback(() => react.OnDamage(0.5f, null, AttackType.Smash))
            .SetUpdate(false)
            .Play();

        completeTween = DOVirtual.DelayedCall(meltFrameTimer, () => react.OnMelt(), false).Play();

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
            .AppendInterval(duration * wakeUpTiming)
            .AppendCallback(() => playerAnim.icedFall.Bool = false)
            .AppendInterval(duration * (1f - wakeUpTiming) * 0.5f)
            .AppendCallback(() => react.OnWakeUp())
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class PlayerTurnL : PlayerCommand
{
    public PlayerTurnL(PlayerCommandTarget target, float duration) : base(target, duration, 0.5f, 0.1f) { }

    protected override bool Action()
    {
        map.TurnLeft();
        mainCamera.TurnLeft();
        playerAnim.turnL.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

        playingTween = tweenMove.TurnToDir.SetEase(Ease.InCubic).OnComplete(mainCamera.ResetCamera).Play();
        return true;
    }
}

public class PlayerTurnR : PlayerCommand
{
    public PlayerTurnR(PlayerCommandTarget target, float duration) : base(target, duration, 0.5f, 0.1f) { }

    protected override bool Action()
    {
        map.TurnRight();
        mainCamera.TurnRight();
        playerAnim.turnR.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

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
        ITile tile = map.ForwardTile;
        Item item = tile.PickItem();

        if (playerAnim.handOn.Bool && itemInventory.PickUp(item.itemInfo))
        {
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

public class PlayerPutItem : PlayerAction
{
    private ItemIcon itemIcon = null;
    public PlayerPutItem(PlayerCommandTarget target, float duration) : base(target, duration) { }

    public PlayerPutItem SetItem(ItemIcon itemIcon)
    {
        this.itemIcon = itemIcon;
        return this;
    }

    protected override bool Action()
    {
        if (playerAnim.handOn.Bool && itemGenerator.Put(itemIcon.itemInfo, map.GetForward, map.dir))
        {
            itemInventory.Remove(itemIcon);
            playerAnim.putItem.Fire();
        }
        else
        {
            return false;
        }
        playerAnim.handOn.Bool = false;
        itemIcon = null;

        return true;
    }
}

public abstract class PlayerAttack : PlayerAction
{
    protected IAttack jab;
    protected IAttack straight;
    protected IAttack kick;

    protected Tween cancelTimer = null;
    protected float cancelStart;

    public PlayerAttack(PlayerCommandTarget target, float duration, float cancelStart = 1f) : base(target, duration)
    {
        jab = target.jab;
        straight = target.straight;
        kick = target.kick;

        this.cancelStart = cancelStart;

        if (cancelStart < 1f)
        {
            cancelTimer = DOTween.Sequence()
                .AppendInterval(cancelStart * duration * FRAME_UNIT)
                .AppendCallback(() => playerAnim.cancel.Bool = true)
                .AppendInterval((1 - cancelStart) * duration * FRAME_UNIT)
                .AppendCallback(() => playerAnim.cancel.Bool = false)
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
            Tween complete = DOTweenTimer(duration, null).OnComplete(o.OnCompleted).Play();

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
        return true;
    }

    protected abstract void Attack();
}

public class PlayerJab : PlayerAttack
{
    public PlayerJab(PlayerCommandTarget target, float duration) : base(target, duration, 0.6f) { }

    protected override void Attack()
    {
        playerAnim.jab.Fire();
        completeTween = jab.AttackSequence(duration).Play();
    }
}

public class PlayerStraight : PlayerAttack
{
    public PlayerStraight(PlayerCommandTarget target, float duration) : base(target, duration, 0.8f) { }

    protected override void Attack()
    {
        playerAnim.straight.Fire();
        completeTween = straight.AttackSequence(duration).Play();
    }
}

public class PlayerKick : PlayerAttack
{
    public PlayerKick(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override void Attack()
    {
        playerAnim.kick.Fire();
        completeTween = kick.AttackSequence(duration).Play();
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
        playingTween = itemInfo.EffectSequence(playerTarget).Play();
        return true;
    }
}

public class PlayerDie : PlayerCommand
{
    public override int priority => 100;
    public PlayerDie(PlayerCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        react.OnDie();
        playerAnim.dieEx.Fire();
        playerTarget.gameOverUI.Play();

        return ExecOnCompleted(() => react.FadeOutToDead());
    }
}

public class PlayerDropFloor : PlayerCommand
{
    public PlayerDropFloor(PlayerCommandTarget target, float duration) : base(target, duration, 1f, 1f) { }

    protected override bool Action()
    {
        SetUIInvisible();
        playerAnim.dropFloor.Fire();
        playingTween = tweenMove.Drop(25.0f, 0f, 0.66f, 1.34f).Play();
        return true;
    }
}

public class PlayerMessage : PlayerAction
{
    MessageData data;
    public PlayerMessage(PlayerCommandTarget target, MessageData data) : base(target, 3.6f)
    {
        this.data = data;
    }

    public PlayerMessage(PlayerCommandTarget target, string[] sentences, FaceID[] faces)
        : this(target, new MessageData(sentences, faces)) { }

    protected override bool Action()
    {
        SetUIInvisible();
        messageController.InputMessageData(data);
        return true;
    }
}
