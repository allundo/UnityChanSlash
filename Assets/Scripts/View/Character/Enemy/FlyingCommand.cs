using DG.Tweening;
using UnityEngine;
using UniRx;
using System;

public abstract class FlyingCommand : EnemyTurnCommand
{
    protected FlyingAnimator flyingAnim;

    public FlyingCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        flyingAnim = target.anim as FlyingAnimator;
    }
}

public class FlyingForward : EnemyForward
{
    public FlyingForward(EnemyCommandTarget target, float duration) : base(target, duration) { }
    protected override bool IsMovable => map.ForwardTile.IsViewOpen;
}

public abstract class FlyingAttack : FlyingCommand
{
    protected IAttack flyingAttack;

    public override int priority => 5;

    protected virtual float attackTimeScale => 0.9f;
    protected virtual float decentVec => -0.5f;

    protected virtual bool IsForwardMovable => map.ForwardTile.IsViewOpen;

    public FlyingAttack(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        flyingAttack = target.enemyAttack[0];
    }
}

public class FlyingAttackStart : FlyingAttack
{
    protected ICommand attackEnd;

    protected virtual ICommand AttackEndCommand(EnemyCommandTarget target, float duration)
        => new FlyingAttackEnd(target, duration);

    protected virtual ICommand PlayNext() => attackEnd;

    public FlyingAttackStart(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        attackEnd = AttackEndCommand(target, duration * (1f - attackTimeScale));
    }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        Vector3 dest = map.CurrentVec3Pos + (map.GetForwardVector() + new Vector3(0f, decentVec, 0f)) * attackTimeScale;

        map.MoveObjectOn(map.GetForward);

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Move(dest, attackTimeScale))
            .Join(tweenMove.DelayedCall(0.75f, () => enemyMap.MoveOnEnemy()))
            .Play();

        flyingAnim.attack.Fire();
        completeTween = flyingAttack.AttackSequence(duration).Play();

        target.input.Interrupt(PlayNext(), false);

        return ObservableComplete(attackTimeScale);
    }
}

public class FlyingAttackEnd : FlyingAttack
{
    public ICommand leave;

    protected virtual bool IsBackwardMovable => map.BackwardTile.IsViewOpen;
    protected virtual bool IsRightMovable => map.RightTile.IsViewOpen;
    protected virtual bool IsLeftMovable => map.LeftTile.IsViewOpen;

    public FlyingAttackEnd(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        leave = new FlyingAttackLeave(target, duration / (1f - attackTimeScale) * 2f);
    }

    public override IObservable<Unit> Execute()
    {
        Vector3 destTileVec = map.DestVec;
        Vector3 dest = map.CurrentVec3Pos + new Vector3(destTileVec.x, decentVec * (1f - attackTimeScale), destTileVec.z);
        var seq = DOTween.Sequence().Join(tweenMove.Move(dest));

        if (IsForwardMovable)
        {
            enemyMap.MoveObjectOn(map.GetForward);
            flyingAnim.leaveF.Fire();
        }
        else if (IsLeftMovable)
        {
            enemyMap.TurnLeft();
            enemyMap.MoveObjectOn(map.GetForward);

            flyingAnim.leaveL.Fire();
            seq.Join(tweenMove.TurnToDir);
        }
        else if (IsRightMovable)
        {
            enemyMap.TurnRight();
            enemyMap.MoveObjectOn(map.GetForward);

            flyingAnim.leaveR.Fire();
            seq.Join(tweenMove.TurnToDir);
        }
        else if (IsBackwardMovable)
        {
            enemyMap.TurnBack();
            enemyMap.MoveObjectOn(map.GetForward);

            flyingAnim.leaveL.Fire();
            seq.Join(tweenMove.TurnLB);
        }

        playingTween = seq.SetUpdate(false).Play();

        input.Interrupt(leave, false);

        return ObservableComplete();
    }
}

public class FlyingAttackLeave : FlyingAttack
{
    public FlyingAttackLeave(EnemyCommandTarget target, float duration) : base(target, duration) { }
    protected override bool Action()
    {
        playingTween = DOTween.Sequence()
            .Join(tweenMove.Move(map.onTilePos, 1f, Ease.OutQuad))
            .Join(tweenMove.DelayedCall(0.51f, () => enemyMap.MoveOnEnemy()))
            .SetUpdate(false)
            .Play();

        return true;
    }
}
public class FlyingIcedFall : FlyingCommand
{
    protected float meltFrameTimer;
    public FlyingIcedFall(EnemyCommandTarget target, float framesToMelt, float duration) : base(target, duration)
    {
        meltFrameTimer = Mathf.Min(framesToMelt, duration + 1f) * FRAME_UNIT;
    }

    public override IObservable<Unit> Execute()
    {
        flyingAnim.speed.Float = 0f;
        flyingAnim.icedFall.Bool = true;


        playingTween = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.JumpRelative(map.DestVec + new Vector3(0f, -1.25f, 0f), 1f, 0f).SetEase(Ease.Linear))
            .AppendCallback(() => mobReact.Damage(0.5f, null, AttackType.Smash))
            .SetUpdate(false)
            .Play();

        completeTween = DOVirtual.DelayedCall(meltFrameTimer, () => mobReact.Melt(), false).Play();

        return ObservableComplete();
    }
}

public class FlyingWakeUp : FlyingCommand
{
    protected float wakeUpTiming;
    public FlyingWakeUp(EnemyCommandTarget target, float duration, float wakeUpTiming = 0.5f) : base(target, duration)
    {
        this.wakeUpTiming = wakeUpTiming;
    }

    protected override bool Action()
    {
        playingTween = DOTween.Sequence()
            .AppendInterval(duration * wakeUpTiming)
            .Append(tweenMove.Move(map.DestVec3Pos, 1f - wakeUpTiming))
            .SetUpdate(false)
            .Play();

        completeTween = DOTween.Sequence()
            .InsertCallback(duration * wakeUpTiming, () => flyingAnim.icedFall.Bool = false)
            .InsertCallback(duration * (1f + wakeUpTiming) * 0.5f, mobReact.OnWakeUp)
            .SetUpdate(false)
            .Play();

        return true;
    }
}
public class FlyingDie : FlyingCommand
{
    public FlyingDie(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = tweenMove.Move(map.DestVec3Pos, 0.5f, Ease.InQuad).Play();
        anim.die.Fire();
        mobReact.OnDie();

        return ExecOnCompleted(() => mobReact.OnDisappear()); // Don't validate input.
    }
}
