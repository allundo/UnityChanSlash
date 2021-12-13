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

    public PlayerCommand(PlayerCommandTarget target, float duration, float validateTiming = 0.5f, float triggerTiming = 0.5f) : base(target, duration, validateTiming)
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

    protected void EnterStair(ITile destTile)
    {
        if (!(destTile is Stair)) return;

        tweenMove.DelayedCall(0.6f, () =>
        {
            playerInput.ClearAll();
            playerInput.SetInputVisible(false);
            GameManager.Instance.EnterStair((destTile as Stair).isUpStair);
        });
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

        EnterStair(DestTile);

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
    public override float Speed => TILE_UNIT / duration;
}

public class PlayerBack : PlayerMove
{
    public PlayerBack(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsBackwardMovable;
    protected override ITile DestTile => map.BackwardTile;
    protected override Pos GetDest => map.GetBackward;
    public override float Speed => -TILE_UNIT / duration;
}

public class PlayerRight : PlayerMove
{
    public PlayerRight(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsRightMovable;
    protected override ITile DestTile => map.RightTile;
    protected override Pos GetDest => map.GetRight;
    public override float RSpeed => TILE_UNIT / duration;
}

public class PlayerLeft : PlayerMove
{
    public PlayerLeft(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsLeftMovable;
    protected override ITile DestTile => map.LeftTile;
    protected override Pos GetDest => map.GetLeft;
    public override float RSpeed => -TILE_UNIT / duration;
}

public class PlayerDashStart : PlayerDash
{
    protected Command dash;
    public PlayerDashStart(PlayerCommandTarget target, float duration) : base(target, duration)
    {
        dash = new PlayerDash(target, duration);
    }

    public override IObservable<Unit> Execute()
    {
        var dest = map.DestVec;
        var timeScale = dest.magnitude / TILE_UNIT;

        playingTween = tweenMove.Linear(map.CurrentVec3Pos + dest, timeScale).OnComplete(hidePlateHandler.Move).Play();

        playerAnim.speed.Float = TILE_UNIT / duration * 0.5f;
        completeTween = DOTween.Sequence()
            .Append(DOTween.To(() => playerAnim.speed.Float, value => playerAnim.speed.Float = value, TILE_UNIT / duration, duration * 0.25f))
            .AppendInterval(duration * (timeScale - 0.25f))
            .AppendCallback(() => playerAnim.speed.Float = 0f)
            .Play();

        target.input.Interrupt(dash, false);

        // EnterStair(map.GetTile(map.CurrentPos));

        return ObservableComplete(timeScale);
    }
}

public class PlayerDash : PlayerCommand
{
    public PlayerDash(PlayerCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    public override IObservable<Unit> Execute()
    {
        if (map.IsForwardMovable)
        {
            playingTween = tweenMove.Linear(map.GetForward).OnComplete(hidePlateHandler.Move).Play();

            playerAnim.speed.Float = TILE_UNIT / duration;
            completeTween = tweenMove.FinallyCall(() => playerAnim.speed.Float = 0).Play();

            target.input.Interrupt(this, false);

            // EnterStair(map.GetTile(map.ForwardPos));

            return ObservableComplete();
        }
        else
        {
            playingTween = tweenMove.BrakeAndBack(2f).Play();

            validateTween = ValidateTween().Play();

            playerAnim.speed.Float = TILE_UNIT / duration;
            completeTween = DOTween.Sequence()
                .AppendCallback(playerAnim.brakeAndBackStep.Fire)
                .Append(DOTween.To(() => playerAnim.speed.Float, value => playerAnim.speed.Float = value, 0, duration))
                .Play();

            return ObservableComplete(2f);
        }
    }
}

public class PlayerBrake : PlayerCommand
{
    public PlayerBrake(PlayerCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    public override IObservable<Unit> Execute()
    {
        validateTween = ValidateTween().Play();
        playerAnim.speed.Float = TILE_UNIT / duration * 2f;

        if (map.IsForwardMovable)
        {
            playingTween = tweenMove.Brake(map.GetForward, 0.6f).OnComplete(hidePlateHandler.Move).Play();

            completeTween = DOTween.Sequence()
                .AppendInterval(duration * 0.2f)
                .AppendCallback(playerAnim.brake.Fire)
                .Append(DOTween.To(
                    () => playerAnim.speed.Float,
                    value => playerAnim.speed.Float = value,
                    0,
                    duration * 0.5f
                ))
                .Play();

            return ObservableComplete();
        }
        else
        {
            playingTween = tweenMove.BrakeAndBack(2f).Play();

            completeTween = DOTween.Sequence()
                .AppendCallback(playerAnim.brakeAndBackStep.Fire)
                .Append(DOTween.To(() => playerAnim.speed.Float, value => playerAnim.speed.Float = value, 0, duration))
                .Play();

            return ObservableComplete(2f);
        }
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

        EnterStair(destTile);

        playerAnim.jump.Fire();

        playingTween = tweenMove
            .Jump(
                distance,
                hidePlateHandler.Move,  // Update HidePlate on entering the next Tile
                hidePlateHandler.Move   // Update HidePlate on entering the next next Tile
            )
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

        playingTween = tweenMove.TurnL.OnComplete(mainCamera.ResetCamera).Play();
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

        playingTween = tweenMove.TurnR.OnComplete(mainCamera.ResetCamera).Play();
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
        playerAnim.handle.Fire();
        return true;
    }
}

public class PlayerGetItem : PlayerAction
{
    public PlayerGetItem(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        ITile tile = map.ForwardTile;
        Item item = tile.PickItem();

        if (itemInventory.PickUp(item))
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
        if (itemGenerator.Put(itemIcon.itemInfo, map.GetForward, map.dir))
        {
            itemInventory.Remove(itemIcon);
            playerAnim.handle.Fire();
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
    public PlayerDie(PlayerCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        map.RemoveObjectOn();
        react.OnDie();
        playerAnim.dieEx.Fire();
        playerTarget.gameOverUI.Play();

        return ExecOnCompleted(() => react.FadeOutOnDead());
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
