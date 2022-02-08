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
    protected readonly float attackTimeScale = 0.9f;

    public override int priority => 5;
    protected float decentVec;

    protected virtual bool IsForwardMovable => map.ForwardTile.IsViewOpen;

    public FlyingAttack(EnemyCommandTarget target, float duration, float decentVec = -0.5f) : base(target, duration)
    {
        flyingAttack = target.enemyAttack[0];
        this.decentVec = decentVec;
    }
}

public class FlyingAttackStart : FlyingAttack
{
    protected ICommand attackEnd;

    protected virtual FlyingAttackEnd AttackEndCommand(EnemyCommandTarget target, float duration)
        => new FlyingAttackEnd(target, duration);

    public FlyingAttackStart(EnemyCommandTarget target, float duration, float decentVec = -0.5f) : base(target, duration, decentVec)
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

        target.input.Interrupt(attackEnd, false);

        return ObservableComplete(attackTimeScale);
    }
}

public class FlyingAttackEnd : FlyingAttack
{
    public ICommand leave;

    protected virtual bool IsBackwardMovable => map.BackwardTile.IsViewOpen;
    protected virtual bool IsRightMovable => map.RightTile.IsViewOpen;
    protected virtual bool IsLeftMovable => map.LeftTile.IsViewOpen;

    public FlyingAttackEnd(EnemyCommandTarget target, float duration, float decentVec = -0.5f) : base(target, duration, decentVec)
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
            seq.Join(tweenMove.TurnL);
        }
        else if (IsRightMovable)
        {
            enemyMap.TurnRight();
            enemyMap.MoveObjectOn(map.GetForward);

            flyingAnim.leaveR.Fire();
            seq.Join(tweenMove.TurnR);
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

        return ObservableComplete(attackTimeScale);
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

public class FlyingDie : FlyingCommand
{
    public FlyingDie(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = tweenMove.Move(map.DestVec3Pos, 0.5f, Ease.InQuad).Play();
        anim.die.Fire();
        react.OnDie();

        return ExecOnCompleted(() => react.FadeOutToDead()); // Don't validate input.
    }
}
