using UnityEngine;
using System;
using DG.Tweening;

public class AnnaSlash : EnemyCommand
{
    protected IAttack enemyAttack;
    protected AnnaAnimator annaAnim;
    protected float preSlashRatio;
    protected float slashRatio;
    public float frames { get; protected set; }

    public AnnaSlash(CommandTarget target, float preDuration, float duration = 160f) : base(target, preDuration + duration, 0.95f)
    {
        enemyAttack = target.Attack(1);
        annaAnim = target.anim as AnnaAnimator;

        frames = preDuration + duration;
        preSlashRatio = preDuration / frames;
        slashRatio = 1f - preSlashRatio;
    }

    protected override bool Action()
    {
        annaAnim.slash.Fire();

        completeTween = DOTween.Sequence()
            .AppendInterval(duration * preSlashRatio)
            .Append(enemyAttack.AttackSequence(duration * slashRatio))
            .Play();

        return true;
    }
}

public class AnnaJumpSlash : AnnaSlash
{
    protected ICommand fallbackCmd;
    protected float crouchingRatio;
    protected float jumpRatio;
    protected virtual float jumpPower => 1f;
    protected virtual Pos DestPos => map.GetForward;
    protected virtual bool FallbackCondition(Pos destPos) => !mobMap.IsMovable(destPos);

    public AnnaJumpSlash(CommandTarget target, AnnaSlash slash, float preDuration, float duration = 160f) : base(target, preDuration, duration)
    {
        fallbackCmd = slash;
        crouchingRatio = (frames - slash.frames) / frames;
        jumpRatio = preSlashRatio - crouchingRatio + 10f / frames;
    }

    protected override bool Action()
    {
        Pos dest = DestPos;

        if (FallbackCondition(dest))
        {
            target.interrupt.OnNext(Data(fallbackCmd));
            return false;
        }

        annaAnim.slash.Fire();
        annaAnim.jumpSlash.Bool = true;

        map.MoveObjectOn(dest);

        playingTween = DOTween.Sequence()
            .AppendInterval(duration * crouchingRatio)
            .Append(tweenMove.Jump(map.onTilePos, jumpRatio, jumpPower))
            .Play();

        completeTween = DOTween.Sequence()
            .AppendInterval(duration * preSlashRatio)
            .Append(enemyAttack.AttackSequence(duration * slashRatio))
            .AppendCallback(() => annaAnim.jumpSlash.Bool = false)
            .Play();

        return true;
    }
}

public class AnnaJumpLeapSlash : AnnaJumpSlash
{
    protected override float jumpPower => 1.5f;
    protected override Pos DestPos => map.GetJump;
    protected override bool FallbackCondition(Pos destPos)
        => !mobMap.IsMovable(destPos) || !mobMap.IsForwardLeapable;

    public AnnaJumpLeapSlash(CommandTarget target, AnnaJumpSlash jumpSlash, float preDuration, float duration = 160f)
        : base(target, jumpSlash, preDuration, duration)
    { }
}

public abstract class AnnaSideWalk : EnemyMove
{
    protected AnnaAnimator annaAnim;
    public AnnaSideWalk(CommandTarget target, float duration) : base(target, duration)
    {
        annaAnim = target.anim as AnnaAnimator;
    }

    protected override void SetSpeed()
    {
        resetTween?.Kill();
        resetTweens.Remove(resetTween);
        speedTween = DOTween.To(() => annaAnim.speedLR.Float, value => annaAnim.speedLR.Float = value, Speed, 0.5f).Play();
    }

    protected override void ResetSpeed()
    {
        speedTween?.Kill();
        resetTween = DOTween.To(() => annaAnim.speedLR.Float, value => annaAnim.speedLR.Float = value, 0f, 0.25f);
        resetTweens.Add(resetTween);
        resetTween.OnComplete(() => resetTweens.Remove(resetTween)).Play();
    }

    public override ICommand GetContinuation()
    {
        CancelValidate();
        try
        {
            return new EnemyMoveContinue(target, Pause(playingTween), Pause(speedTween), resetTween, RemainingDuration, onCompleted);
        }
        catch (NullReferenceException ex)
        {
            Debug.Log($"target: {target}, playingTween: {playingTween}, speed: {speedTween}, reset: {resetTween}, onCompleted: {onCompleted}");
            Debug.Log(ex.Message);
            return null;
        }
    }
}

public class AnnaLeftMove : AnnaSideWalk
{
    public AnnaLeftMove(CommandTarget target, float duration) : base(target, duration)
    { }

    protected override bool IsMovable => mobMap.IsLeftMovable;
    protected override Pos GetDest => mobMap.GetLeft;
    protected override float Speed => -TILE_UNIT / duration;
}

public class AnnaRightMove : AnnaSideWalk
{
    public AnnaRightMove(CommandTarget target, float duration) : base(target, duration)
    { }

    protected override bool IsMovable => mobMap.IsRightMovable;
    protected override Pos GetDest => mobMap.GetRight;
    protected override float Speed => TILE_UNIT / duration;
}

public class AnnaBackStep : EnemyCommand
{
    protected AnnaAnimator annaAnim;
    protected override float Speed => -TILE_UNIT / duration;

    public AnnaBackStep(CommandTarget target, float duration) : base(target, duration)
    {
        annaAnim = target.anim as AnnaAnimator;
    }

    protected override bool Action()
    {
        if (!enemyMap.IsBackwardMovable) return false;

        annaAnim.speed.Float = Speed;

        Pos destPos = map.GetBackward;

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1f, 0.1f, 0.5f)
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        completeTween = tweenMove
            .FinallyCall(() => annaAnim.speed.Float = 0f)
            .Play();

        return true;
    }
}

public class AnnaBackLeap : AnnaBackStep
{
    protected ICommand backStep;
    protected override float Speed => -TILE_UNIT * 2f / duration;

    public AnnaBackLeap(CommandTarget target, float duration, ICommand backStep) : base(target, duration)
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

        annaAnim.speed.Float = Speed;

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1.5f, 0.1f, 0.5f)
            .InsertCallback(0.16f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        completeTween = tweenMove
            .FinallyCall(() => annaAnim.speed.Float = 0f)
            .Play();

        return true;
    }
}

public class AnnaJumpLeap : EnemyCommand
{
    protected AnnaAnimator annaAnim;

    public AnnaJumpLeap(CommandTarget target, float duration) : base(target, duration)
    {
        annaAnim = target.anim as AnnaAnimator;
    }

    protected override bool Action()
    {
        Pos destPos = mobMap.GetJump;

        if (!mobMap.IsForwardLeapable || !mobMap.IsMovable(destPos)) return false;

        annaAnim.jump.Bool = true;

        enemyMap.MoveObjectOn(destPos);

        playingTween = tweenMove.SimpleLeap(destPos, 1.5f, 0.1f, 0.5f)
            .InsertCallback(0.16f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        completeTween = tweenMove
            .FinallyCall(() => annaAnim.jump.Bool = false)
            .Play();

        return true;
    }
}
