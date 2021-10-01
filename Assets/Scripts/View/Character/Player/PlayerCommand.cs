using UnityEngine;
using System;
using UniRx;
using DG.Tweening;

public abstract class PlayerCommand : Command
{
    protected PlayerAnimator playerAnim;
    protected ThirdPersonCamera mainCamera;
    protected HidePlateHandler hidePlateHandler;
    protected ItemGenerator itemGenerator;
    protected ItemIconGenerator itemIconGenerator;
    protected MessageController messageController;
    protected GameOverUI gameOverUI;

    protected Tween validateTrigger;

    protected IObserver<Unit> onValidateTrigger;
    protected IObserver<Unit> onClearAll;
    protected IObserver<bool> onUIVisible;

    public PlayerCommand(PlayerCommander commander, float duration) : base(commander, duration)
    {
        playerAnim = anim as PlayerAnimator;
        mainCamera = commander.mainCamera;
        hidePlateHandler = commander.hidePlateHandler;
        itemGenerator = commander.itemGenerator;
        itemIconGenerator = commander.itemIconGenerator;
        messageController = commander.messageController;
        gameOverUI = commander.gameOverUI;
        onValidateTrigger = commander.onValidateTrigger;
        onClearAll = commander.onClearAll;
        onUIVisible = commander.onUIVisible;
    }

    protected override void SetValidateTimer(float timing = 0.5f)
    {
        validateTween = tweenMove.SetDelayedCall(timing, () => onValidateInput.OnNext(true));
    }

    protected void SetValidateTimer(float timing, float triggerTiming)
    {
        base.SetValidateTimer(timing);
        SetValidateTriggerTimer(triggerTiming);
    }

    protected void SetValidateTriggerTimer(float timing = 0.5f)
    {
        validateTrigger = tweenMove.SetDelayedCall(timing, () => onValidateTrigger.OnNext(Unit.Default));
    }

    public override void Cancel()
    {
        base.Cancel();
        validateTrigger?.Kill();
    }

    public override void CancelValidateTween()
    {
        validateTween?.Kill();
        validateTrigger?.Kill();
    }

    protected void EnterStair(ITile destTile)
    {
        if (!(destTile is Stair)) return;


        tweenMove.SetDelayedCall(0.6f, () =>
        {
            GameManager.Instance.EnterStair((destTile as Stair).isUpStair);
            onClearAll.OnNext(Unit.Default);
            playerAnim.Pause();
        });
    }
}

public abstract class PlayerMove : PlayerCommand
{
    protected ITile destTile;
    public PlayerMove(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected abstract bool IsMovable { get; }
    protected abstract Vector3 Dest { get; }
    protected abstract ITile DestTile { get; }
    protected Vector3 startPos = default;

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
        map.ResetOnCharactor(startPos + Dest);
    }

    public override void Execute()
    {
        if (!IsMovable)
        {
            onValidateInput.OnNext(true);
            onCompleted.OnNext(Unit.Default);
            return;
        }

        EnterStair(DestTile);

        startPos = map.CurrentVec3Pos;
        map.SetOnCharactor(startPos + Dest);
        map.ResetOnCharactor(startPos);

        SetSpeed();
        PlayTween(tweenMove.GetLinearMove(Dest), () =>
        {
            hidePlateHandler.Move();
            ResetSpeed();
        });

        SetValidateTimer(0.95f, 0.5f);
    }
}

public class PlayerForward : PlayerMove
{
    public PlayerForward(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override bool IsMovable => map.IsForwardMovable;
    protected override ITile DestTile => map.ForwardTile;
    protected override Vector3 Dest => map.GetForwardVector();
    public override float Speed => TILE_UNIT / duration;
}

public class PlayerBack : PlayerMove
{
    public PlayerBack(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override bool IsMovable => map.IsBackwardMovable;
    protected override ITile DestTile => map.BackwardTile;
    protected override Vector3 Dest => map.GetBackwardVector();
    public override float Speed => -TILE_UNIT / duration;
}

public class PlayerRight : PlayerMove
{
    public PlayerRight(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override bool IsMovable => map.IsRightMovable;
    protected override ITile DestTile => map.RightTile;
    protected override Vector3 Dest => map.GetRightVector();
    public override float RSpeed => TILE_UNIT / duration;
}

public class PlayerLeft : PlayerMove
{
    public PlayerLeft(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override bool IsMovable => map.IsLeftMovable;
    protected override ITile DestTile => map.LeftTile;
    protected override Vector3 Dest => map.GetLeftVector();
    public override float RSpeed => -TILE_UNIT / duration;
}

public class PlayerJump : PlayerCommand
{
    public PlayerJump(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected Vector3 dest = default;
    protected Vector3 startPos = default;

    public override void Cancel()
    {
        base.Cancel();
        map.ResetOnCharactor(startPos + dest);
    }

    public override void Execute()
    {
        int distance = map.IsJumpable ? 2 : map.IsForwardMovable ? 1 : 0;

        ITile destTile =
            distance == 2 ? map.JumpTile :
            distance == 1 ? map.ForwardTile :
            null;

        EnterStair(destTile);

        dest = map.GetForwardVector(distance);

        startPos = map.CurrentVec3Pos;
        map.SetOnCharactor(startPos + dest);
        map.ResetOnCharactor(startPos);

        playerAnim.jump.Fire();

        // 2マス進む場合は途中で天井の状態を更新
        if (distance == 2)
        {
            tweenMove.SetDelayedCall(0.4f, () => hidePlateHandler.Move());
        }

        SetValidateTimer();

        PlayTween(tweenMove.GetJumpSequence(dest), () =>
        {
            if (distance > 0) hidePlateHandler.Move();
        });
    }
}

public class PlayerTurnL : PlayerCommand
{
    public PlayerTurnL(PlayerCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        map.TurnLeft();
        mainCamera.TurnLeft();
        playerAnim.turnL.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

        SetValidateTimer(0.5f, 0.1f);
        PlayTween(tweenMove.GetRotate(-90), () => mainCamera.ResetCamera());
    }
}

public class PlayerTurnR : PlayerCommand
{
    public PlayerTurnR(PlayerCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        map.TurnRight();
        mainCamera.TurnRight();
        playerAnim.turnR.Fire();
        hidePlateHandler.Turn();
        itemGenerator.Turn(map.dir);

        SetValidateTimer(0.5f, 0.1f);
        PlayTween(tweenMove.GetRotate(90), () => mainCamera.ResetCamera());
    }
}
public abstract class PlayerAction : PlayerCommand
{
    protected float timing;
    public PlayerAction(PlayerCommander commander, float duration, float timing = 0.5f) : base(commander, duration)
    {
        this.timing = timing;
    }

    public override void Execute()
    {
        Action();

        SetValidateTimer(timing, timing * 0.5f);
        SetDispatchFinal();
    }

    protected abstract void Action();
}

public class PlayerHandle : PlayerAction
{
    public PlayerHandle(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        playerAnim.handle.Fire();
    }
}

public class PlayerGetItem : PlayerAction
{
    public PlayerGetItem(PlayerCommander commander, float duration) : base(commander, duration) { }

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

    public PlayerAttack(PlayerCommander commander, float duration, float timing = 0.5f) : base(commander, duration, timing)
    {
        jab = commander.jab;
        straight = commander.straight;
        kick = commander.kick;
    }
}

public class PlayerJab : PlayerAttack
{
    public PlayerJab(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        playerAnim.attack.Fire();
        playingTween = jab.SetAttack(duration);
    }
}

public class PlayerStraight : PlayerAttack
{
    public PlayerStraight(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        playerAnim.straight.Fire();
        playingTween = straight.SetAttack(duration);
    }
}
public class PlayerKick : PlayerAttack
{
    public PlayerKick(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        playerAnim.kick.Fire();
        playingTween = kick.SetAttack(duration);
    }
}

public class PlayerDie : PlayerCommand
{
    public PlayerDie(PlayerCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        map.ResetOnCharactor();
        playerAnim.dieEx.Fire();
        gameOverUI.Play();
        SetDestoryFinal();
    }
}

public class PlayerDropFloor : PlayerCommand
{
    public PlayerDropFloor(PlayerCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        onUIVisible.OnNext(false);
        SetValidateTimer(1f, 1f);

        playerAnim.dropFloor.Fire();
        PlayTween(tweenMove.GetDropMove(25.0f, 0f, 0.66f, 1.34f), () => onUIVisible.OnNext(true));
    }
}
public class PlayerStartMessage : PlayerAction
{
    public PlayerStartMessage(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        MessageData data = new MessageData
        {
            sentences = new string[]
            {
                    "[仮] (ここに開幕の説明が入ります)",
                    "[仮] ・・・メタすぎる！"
            },
            faces = new FaceID[]
            {
                    FaceID.DISATTRACT,
                    FaceID.ANGRY
            }
        };

        messageController.InputMessageData(data);
    }
}

public class PlayerRestartMessage : PlayerAction
{
    public PlayerRestartMessage(PlayerCommander commander, float duration) : base(commander, duration) { }

    protected override void Action()
    {
        MessageData data = new MessageData
        {
            sentences = new string[]
            {
                    "[仮] ・・・という夢だったのさ",
                    "[仮] なんも解決してないんだけどねっ！",
            },
            faces = new FaceID[]
            {
                    FaceID.SMILE,
                    FaceID.ANGRY
            }
        };

        messageController.InputMessageData(data);
    }
}
