using System;
using UniRx;
using DG.Tweening;

public abstract class PlayerCommand : Command
{
    protected PlayerCommandTarget playerTarget;
    protected PlayerAnimator playerAnim;
    protected ThirdPersonCamera mainCamera;
    protected HidePlateHandler hidePlateHandler;
    protected ItemGenerator itemGenerator;
    protected ItemIconGenerator itemIconGenerator;
    protected MessageController messageController;

    protected Tween validateTrigger;

    protected IObserver<Unit> onValidateTrigger;

    protected float triggerInvalidDuration;

    public PlayerCommand(PlayerCommandTarget target, float duration, float validateTiming = 0.5f, float triggerTiming = 0.5f) : base(target, duration, validateTiming)
    {
        playerTarget = target;
        this.triggerInvalidDuration = this.duration * triggerTiming;

        playerAnim = anim as PlayerAnimator;
        mainCamera = target.mainCamera;
        hidePlateHandler = target.hidePlateHandler;
        itemGenerator = target.itemGenerator;
        itemIconGenerator = target.itemIconGenerator;
        messageController = target.messageController;
    }

    protected override IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        Action();
        return AddTriggerValidation(execObservable);
    }

    protected IObservable<bool> AddTriggerValidation(IObservable<bool> execObservable)
    {
        if (triggerInvalidDuration >= invalidDuration) return execObservable;
        return execObservable.Merge(DOTweenTimer(triggerInvalidDuration, true));
    }

    protected void SetUIInvisible()
    {
        Action<bool> Visible = playerTarget.onUIVisible.OnNext;

        Visible(false);
        onCompleted.Add(() => Visible(true));
    }

    protected void EnterStair(ITile destTile)
    {
        if (!(destTile is Stair)) return;

        tweenMove.SetDelayedCall(0.6f, () =>
        {
            playerTarget.onClearAll.OnNext(Unit.Default);
            playerTarget.onUIVisible.OnNext(false);
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
    protected Pos prevPos = new Pos();

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

    public override void Cancel()
    {
        base.Cancel();
        var destPos = map.MoveOnCharacter(prevPos);
    }

    protected override IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        if (!IsMovable)
        {
            Cancel();
            return Observable.Return(false);
        }

        EnterStair(DestTile);

        prevPos = map.CurrentPos;
        var destPos = map.MoveOnCharacter(GetDest);

        SetSpeed();

        playingTween =
            tweenMove.GetLinearMove(map.WorldPos(destPos))
                .OnComplete(() =>
                {
                    hidePlateHandler.Move();
                    ResetSpeed();
                })
                .Play();

        return AddTriggerValidation(execObservable);
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

public class PlayerJump : PlayerCommand
{
    public PlayerJump(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected Pos prevPos = new Pos();

    public override void Cancel()
    {
        base.Cancel();
        map.MoveOnCharacter(prevPos);
    }

    protected override void Action()
    {
        int distance = 0;
        ITile destTile = null;
        Pos destPos = prevPos = map.CurrentPos;

        if (map.IsJumpable)
        {
            distance = 2;
            destTile = map.JumpTile;
            destPos = map.GetJump;
        }
        else if (map.IsForwardMovable)
        {
            distance = 1;
            destTile = map.ForwardTile;
            destPos = map.GetForward;
        }

        EnterStair(destTile);

        map.MoveOnCharacter(destPos);

        playerAnim.jump.Fire();

        // 2マス進む場合は途中で天井の状態を更新
        if (distance == 2)
        {
            tweenMove.SetDelayedCall(0.4f, () => hidePlateHandler.Move());
        }

        playingTween =
            tweenMove.GetJumpSequence(map.GetForwardVector(distance))
                .OnComplete(() =>
                {
                    if (distance > 0) hidePlateHandler.Move();
                })
                .Play();
    }
}

public class PlayerTurnL : PlayerCommand
{
    public PlayerTurnL(PlayerCommandTarget target, float duration) : base(target, duration, 0.5f, 0.1f) { }

    protected override void Action()
    {
        map.TurnLeft();
        mainCamera.TurnLeft();
        playerAnim.turnL.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

        playingTween = tweenMove.TurnL.OnComplete(mainCamera.ResetCamera).Play();
    }
}

public class PlayerTurnR : PlayerCommand
{
    public PlayerTurnR(PlayerCommandTarget target, float duration) : base(target, duration, 0.5f, 0.1f) { }

    protected override void Action()
    {
        map.TurnRight();
        mainCamera.TurnRight();
        playerAnim.turnR.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

        playingTween = tweenMove.TurnR.OnComplete(mainCamera.ResetCamera).Play();
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

    protected override void Action() => playerAnim.handle.Fire();
}

public class PlayerGetItem : PlayerAction
{
    public PlayerGetItem(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        ITile tile = map.ForwardTile;
        Item item = tile.PickItem();

        if (itemIconGenerator.PickUp(item))
        {
            playerAnim.getItem.Fire();
        }
        else
        {
            tile.PutItem(item);
        }
    }
}

public abstract class PlayerAttack : PlayerAction
{
    protected MobAttack jab;
    protected MobAttack straight;
    protected MobAttack kick;

    public PlayerAttack(PlayerCommandTarget target, float duration, float timing = 0.5f) : base(target, duration, timing)
    {
        jab = target.jab;
        straight = target.straight;
        kick = target.kick;
    }
}

public class PlayerJab : PlayerAttack
{
    public PlayerJab(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        playerAnim.attack.Fire();
        playingTween = jab.AttackSequence(duration).Play();
    }
}

public class PlayerStraight : PlayerAttack
{
    public PlayerStraight(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        playerAnim.straight.Fire();
        playingTween = straight.AttackSequence(duration).Play();
    }
}
public class PlayerKick : PlayerAttack
{
    public PlayerKick(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        playerAnim.kick.Fire();
        playingTween = kick.AttackSequence(duration).Play();
    }
}

public class PlayerDie : PlayerCommand
{
    public PlayerDie(PlayerCommandTarget target, float duration) : base(target, duration) { }

    protected override IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        map.ResetOnCharacter();
        playerAnim.dieEx.Fire();
        playerTarget.gameOverUI.Play();

        NotifyOnDeadFinal();
        return null;
    }
}

public class PlayerDropFloor : PlayerCommand
{
    public PlayerDropFloor(PlayerCommandTarget target, float duration) : base(target, duration, 1f, 1f) { }

    protected override void Action()
    {
        SetUIInvisible();
        playerAnim.dropFloor.Fire();
        playingTween = tweenMove.GetDropMove(25.0f, 0f, 0.66f, 1.34f).Play();
    }
}

public class PlayerMessage : PlayerAction
{
    MessageData data;
    public PlayerMessage(PlayerCommandTarget target, MessageData data) : base(target, 0.1f)
    {
        this.data = data;
    }

    public PlayerMessage(PlayerCommandTarget target, string[] sentences, FaceID[] faces)
        : this(target, new MessageData(sentences, faces)) { }

    protected override void Action()
    {
        SetUIInvisible();
        messageController.InputMessageData(data);
    }
}
