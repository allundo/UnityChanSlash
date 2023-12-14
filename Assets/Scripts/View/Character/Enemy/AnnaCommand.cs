using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;

public abstract class AnnaSpeed : EnemyCommand
{
    protected AnnaAnimator annaAnim;
    protected MobAnimator.AnimatorBool SpdCmd => annaAnim.speedCommand;
    protected virtual MobAnimator.AnimatorFloat SpeedValue => annaAnim.speed;

    public AnnaSpeed(AnnaCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        annaAnim = target.anim as AnnaAnimator;
    }

    protected virtual void StartMoving()
    {
        SpdCmd.Bool = true;
        SpeedValue.Float = Speed;
    }

    protected virtual void EndMoving()
    {
        SpdCmd.Bool = false;
        SpeedValue.Float = 0f;
    }
}

public abstract class AnnaMove : AnnaSpeed
{
    protected AnnaCommandTarget spdTarget;
    protected virtual Action<Tween> SetSpeed => spdTarget.SetSpeed;
    protected abstract bool IsMovable { get; }
    protected abstract Pos DestPos { get; }

    public AnnaMove(AnnaCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        spdTarget = target;
    }

    protected override void StartMoving()
    {
        SpdCmd.Bool = true;
        SetSpeed(DOTween.To(() => SpeedValue.Float, value => SpeedValue.Float = value, Speed, 0.5f));
    }

    protected override void EndMoving()
    {
        SpdCmd.Bool = false;
        SetSpeed(DOTween.To(() => SpeedValue.Float, value => SpeedValue.Float = value, 0f, 0.25f));
    }

    public override void Cancel()
    {
        SetSpeed(null);
        SpeedValue.Float = 0f;
        SpdCmd.Bool = false;
        base.Cancel();
    }

    public override ICommand GetContinuation()
    {
        CancelValidate();
        try
        {
            return new AnnaMoveContinue(spdTarget, Pause(playingTween), RemainingDuration, onCompleted);
        }
        catch (NullReferenceException ex)
        {
            Debug.Log($"target: {target}, playingTween: {playingTween}, speed: {spdTarget.speedTween}, onCompleted: {onCompleted}");
            Debug.Log(ex.Message);
            return null;
        }
    }

    protected override bool Action()
    {
        if (!IsMovable) return false;

        Pos pos = DestPos;

        mobMap.MoveObjectOn(pos);
        StartMoving();

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Move(pos))
            .Join(tweenMove.DelayedCall(0.51f, () => enemyMap.MoveOnEnemy()))
            .AppendCallback(EndMoving)
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class AnnaMoveContinue : Command
{
    protected AnnaCommandTarget spdTarget;
    protected AnnaAnimator annaAnim;
    protected Tween speedTween;
    protected Tween speedTweenLR;

    public AnnaMoveContinue(AnnaCommandTarget target, Tween playing, float duration, List<Action> onCompleted = null)
        : base(target, playing, null, duration, onCompleted)
    {
        spdTarget = target;
        annaAnim = target.anim as AnnaAnimator;
        speedTween = Pause(target.speedTween);
        speedTweenLR = Pause(target.speedTweenLR);
    }

    protected override bool Action()
    {
        spdTarget.SetSpeed(speedTween);
        spdTarget.SetSpeedLR(speedTweenLR);

        if (duration == 0f) return false;

        annaAnim.speedCommand.Bool = true;
        playingTween?.Play();

        return true;
    }

    public override ICommand GetContinuation()
    {
        CancelValidate();
        return new AnnaMoveContinue(spdTarget, Pause(playingTween), RemainingDuration, onCompleted);
    }

    public override void Cancel()
    {
        spdTarget.SetSpeed(null);
        spdTarget.SetSpeedLR(null);
        annaAnim.speed.Float = annaAnim.speedLR.Float = 0f;
        annaAnim.speedCommand.Bool = false;
        base.Cancel();
    }
}

public class AnnaForward : AnnaMove
{
    protected override Pos DestPos => map.GetForward;
    protected override bool IsMovable => mobMap.IsMovable(DestPos);
    protected override float Speed => TILE_UNIT / duration * 1.25f; // Adjust max speed to 6
    public AnnaForward(AnnaCommandTarget target, float duration) : base(target, duration) { }
}

public class AnnaBackward : AnnaMove
{
    protected override Pos DestPos => map.GetForward;
    protected override bool IsMovable => mobMap.IsMovable(DestPos);
    protected override float Speed => -TILE_UNIT / duration;
    public AnnaBackward(AnnaCommandTarget target, float duration) : base(target, duration) { }
}

public abstract class AnnaSideMove : AnnaMove
{
    public AnnaSideMove(AnnaCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsMovable(DestPos);
    protected override MobAnimator.AnimatorFloat SpeedValue => annaAnim.speedLR;
    protected override Action<Tween> SetSpeed => spdTarget.SetSpeedLR;
}

public class AnnaLeftMove : AnnaSideMove
{
    protected override Pos DestPos => map.GetLeft;
    protected override float Speed => -TILE_UNIT / duration;
    public AnnaLeftMove(AnnaCommandTarget target, float duration) : base(target, duration) { }
}

public class AnnaRightMove : AnnaSideMove
{
    protected override Pos DestPos => map.GetRight;
    protected override float Speed => TILE_UNIT / duration;
    public AnnaRightMove(AnnaCommandTarget target, float duration) : base(target, duration) { }
}

public class AnnaSlash : EnemyCommand
{
    protected AnnaCommandTarget spdTarget;
    protected IAttack slash;
    protected AnnaAnimator annaAnim;
    protected float preSlashRatio;
    protected float slashRatio;
    public float frames { get; protected set; }

    protected void StartJump()
    {
        spdTarget.SetSpeed(null);
        annaAnim.speed.Float = 0f;
        annaAnim.jump.Bool = true;
    }

    public AnnaSlash(AnnaCommandTarget target, float preDuration, float duration = 160f) : base(target, preDuration + duration, 0.95f)
    {
        spdTarget = target;
        slash = target.Attack(1);
        annaAnim = target.anim as AnnaAnimator;

        frames = preDuration + duration;
        preSlashRatio = preDuration / frames;
        slashRatio = 1f - preSlashRatio;
    }

    protected override bool Action()
    {
        annaAnim.slash.Fire();

        completeTween = slash.AttackSequence(duration * slashRatio)
            .SetDelay(duration * preSlashRatio)
            .Play();

        return true;
    }
}

public class AnnaJumpSlash : AnnaSlash
{
    protected AnnaSlash slashCmd;
    protected float crouchingRatio;
    protected float jumpRatio;
    public float SlashFrames => slashCmd.frames;

    public AnnaJumpSlash(AnnaCommandTarget target, AnnaSlash slashCmd, float preDuration, float duration = 160f) : base(target, preDuration, duration)
    {
        this.slashCmd = slashCmd;
        crouchingRatio = (frames - slashCmd.frames) / frames;
        jumpRatio = preSlashRatio - crouchingRatio + 10f / frames;
    }

    protected override bool Action()
    {
        Pos destPos = map.GetForward;

        if (!mobMap.IsMovable(destPos))
        {
            target.interrupt.OnNext(Data(slashCmd));
            return false;
        }

        annaAnim.slash.Fire();
        StartJump();

        map.MoveObjectOn(destPos);

        playingTween = DOTween.Sequence()
            .AppendInterval(duration * crouchingRatio)
            .Append(tweenMove.Jump(map.onTilePos, jumpRatio, 1f))
            .AppendCallback(() => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        completeTween = slash.AttackSequence(duration * slashRatio)
            .InsertCallback(0, () => annaAnim.jump.Bool = false)
            .SetDelay(duration * preSlashRatio)
            .Play();

        return true;
    }
}

public class AnnaJumpLeapSlash : AnnaSlash
{
    protected ICommand fallbackCmd;
    protected float crouchingRatio;
    protected float jumpRatio;

    public AnnaJumpLeapSlash(AnnaCommandTarget target, AnnaJumpSlash jumpSlash, float preDuration, float duration = 160f) : base(target, preDuration, duration)
    {
        fallbackCmd = jumpSlash;
        crouchingRatio = (frames - jumpSlash.SlashFrames) / frames;
        jumpRatio = preSlashRatio - crouchingRatio + 10f / frames;
    }

    protected override bool Action()
    {
        Pos destPos = map.GetJump;

        if (!mobMap.IsMovable(destPos) || !mobMap.IsForwardLeapable)
        {
            target.interrupt.OnNext(Data(fallbackCmd));
            return false;
        }

        annaAnim.slash.Fire();
        StartJump();

        map.MoveObjectOn(destPos);

        playingTween = DOTween.Sequence()
            .AppendInterval(duration * crouchingRatio)
            .Append(tweenMove.Jump(map.onTilePos, jumpRatio, 1.5f))
            .AppendCallback(() => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        completeTween = slash.AttackSequence(duration * slashRatio)
            .InsertCallback(0, () => annaAnim.jump.Bool = false)
            .SetDelay(duration * preSlashRatio)
            .Play();

        return true;
    }
}

public abstract class AnnaJump : EnemyCommand
{
    protected AnnaAnimator annaAnim;
    protected Action<Tween> setSpeed;

    public AnnaJump(AnnaCommandTarget target, float duration) : base(target, duration)
    {
        annaAnim = target.anim as AnnaAnimator;
        setSpeed = target.SetSpeed;
    }

    protected void StartMoving()
    {
        setSpeed(null);
        annaAnim.speed.Float = Speed;
        annaAnim.jump.Bool = true;
    }

    protected void EndMoving()
    {
        annaAnim.speed.Float = 0f;
        annaAnim.jump.Bool = false;
    }
}

public class AnnaBackStep : AnnaJump
{
    protected override float Speed => -4.0f;

    public AnnaBackStep(AnnaCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsBackwardMovable) return false;

        Pos destPos = map.GetBackward;

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1f, 0.05f, 0.4f)
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        StartMoving();
        completeTween = tweenMove.FinallyCall(EndMoving).Play();

        return true;
    }
}

public class AnnaBackLeap : AnnaBackStep
{
    protected ICommand backStep;
    protected override float Speed => -8.0f;

    public AnnaBackLeap(AnnaCommandTarget target, float duration, ICommand backStep) : base(target, duration)
    {
        this.backStep = backStep;
    }

    protected override bool Action()
    {
        Pos backward = mobMap.GetBackward;
        Pos destPos = map.dir.GetBackward(backward);

        if (!mobMap.IsLeapable(backward) || !mobMap.IsMovable(destPos))
        {
            target.interrupt.OnNext(Data(backStep));
            return false;
        }

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1.5f, 0.05f, 0.4f)
            .InsertCallback(0.3f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.5f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        StartMoving();
        completeTween = tweenMove.FinallyCall(EndMoving).Play();

        return true;
    }
}

public class AnnaJumpLeap : AnnaJump
{
    protected override float Speed => 8.0f;
    public AnnaJumpLeap(AnnaCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        Pos destPos = mobMap.GetJump;

        if (!mobMap.IsForwardLeapable || !mobMap.IsMovable(destPos)) return false;

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1.5f, 0.05f, 0.4f)
            .InsertCallback(0.3f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.5f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        StartMoving();
        completeTween = tweenMove.FinallyCall(EndMoving).Play();

        return true;
    }
}

public class AnnaIcedFall : EnemyCommand, IIcedCommand
{
    protected float meltTime;
    public float framesToMelt { get; protected set; }

    public AnnaIcedFall(CommandTarget target, float framesToMelt, float duration) : base(target, duration)
    {
        this.framesToMelt = framesToMelt;
        meltTime = Mathf.Min(framesToMelt, duration + 1f) * FRAME_UNIT;
    }

    public override IObservable<Unit> Execute()
    {
        Vector3 dest = mobMap.DestVec;                     // Remaining vector to front tile
        float height = Mathf.Abs(dest.y);
        Vector3 horizontalVec = new Vector3(dest.x, 0f, dest.z);
        float minDropDuration = 0.25f;
        float layDownHeight = 0.25f;
        // t = sqrt(h * 2/9.8)[sec]
        float dropSec = Mathf.Max(Mathf.Sqrt((height + layDownHeight) * 0.2041f), minDropDuration);

        (target.anim as AnnaAnimator).icedFall.Bool = true;
        mobReact.Iced(framesToMelt, false);

        // Reset OnEnemy tile to destination
        enemyMap.MoveOnEnemy(map.onTilePos);

        playingTween = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.SimpleArc(horizontalVec, -height, dropSec / duration))
            .AppendCallback(() => mobReact.Damage(5f, map.dir, AttackType.Smash))
            .SetUpdate(false)
            .Play();

        completeTween = DOVirtual.DelayedCall(meltTime, () => mobReact.Melt(), false).Play();

        return ObservableComplete();
    }
}

public class AnnaWakeUp : EnemyCommand
{
    protected float wakeUpTiming;
    protected AnnaAnimator annaAnim;
    public AnnaWakeUp(CommandTarget target, float duration, float wakeUpTiming = 0.5f) : base(target, duration)
    {
        this.wakeUpTiming = wakeUpTiming;
        annaAnim = target.anim as AnnaAnimator;
    }

    protected override bool Action()
    {
        completeTween = DOTween.Sequence()
            .InsertCallback(duration * wakeUpTiming, () => (target.anim as AnnaAnimator).icedFall.Bool = false)
            .InsertCallback(duration * (1f + wakeUpTiming) * 0.5f, mobReact.OnWakeUp)
            .SetUpdate(false)
            .Play();

        return true;
    }
}
