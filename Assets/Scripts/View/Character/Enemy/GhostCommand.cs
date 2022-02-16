using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class GhostForward : EnemyForward
{
    public GhostForward(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (base.Action())
        {
            (react as IGhostReactor).OnAppear();
            return true;
        }
        return false;
    }
}

public class GhostThrough : EnemyForward
{
    ICommand throughEnd;
    ICommand attack;
    public GhostThrough(EnemyCommandTarget target, float duration, ICommand attack) : base(target, duration)
    {
        throughEnd = new GhostThroughEnd(target, duration);
        this.attack = attack;
    }

    public override IObservable<Unit> Execute()
    {
        playingTween = LinearMove(GetDest);
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        SetSpeed();
        (react as IGhostReactor).OnHide();

        input.Interrupt(MapUtil.IsOnPlayer(map.GetForward) ? attack : throughEnd, false);

        return ObservableComplete();
    }
}

public class GhostThroughEnd : EnemyForward
{
    public GhostThroughEnd(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!map.ForwardTile.IsViewOpen) return false;
        if (map.IsForwardMovable) (react as IGhostReactor).OnAppear();

        playingTween = LinearMove(GetDest);
        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        return true;
    }
}

public class GhostAttackStart : FlyingAttackStart
{
    protected ICommand attackKeep;

    protected override bool IsForwardMovable => map.ForwardTile.IsViewOpen;

    protected override ICommand AttackEndCommand(EnemyCommandTarget target, float duration)
        => attackEnd ?? new GhostAttackEnd(target, duration);

    protected override ICommand PlayNext()
        => MapUtil.IsOnPlayer(map.GetForward) ? attackKeep : attackEnd;

    protected override float attackTimeScale => 0.75f;
    protected override float decentVec => -0.1f;

    public GhostAttackStart(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        attackEnd = AttackEndCommand(target, duration);
        attackKeep = new GhostAttackKeep(target, duration, attackEnd);
    }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        (react as IGhostReactor).OnAttackStart();
        return base.Execute();
    }
}

public class GhostAttackKeep : FlyingAttack
{
    protected ICommand attackEnd;

    protected ICommand PlayNext() => MapUtil.IsOnPlayer(map.GetForward) ? this : attackEnd;

    public GhostAttackKeep(EnemyCommandTarget target, float duration, ICommand attackEnd) : base(target, duration)
    {
        this.attackEnd = attackEnd;
    }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Execute AttackEnd
            input.Interrupt(attackEnd, false);
            return Observable.Empty(Unit.Default);
        }

        Vector3 dest = map.CurrentVec3Pos + map.GetForwardVector();

        map.MoveObjectOn(map.GetForward);

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Move(dest))
            .Join(tweenMove.DelayedCall(0.75f, () => enemyMap.MoveOnEnemy()))
            .Play();

        flyingAnim.attack.Fire();
        completeTween = flyingAttack.AttackSequence(duration).Play();

        target.input.Interrupt(PlayNext(), false);

        return ObservableComplete();
    }
}

public class GhostAttackEnd : FlyingAttackEnd
{
    protected override bool IsForwardMovable => map.ForwardTile.IsViewOpen;
    protected override bool IsBackwardMovable => map.IsBackwardMovable;
    protected override bool IsRightMovable => map.IsRightMovable;
    protected override bool IsLeftMovable => map.IsLeftMovable;

    protected override float attackTimeScale => 0.75f;
    protected override float decentVec => -0.1f;

    protected override FlyingAttackLeave AttackLeaveCommand(EnemyCommandTarget target, float duration)
        => new GhostAttackLeave(target, duration);
    public GhostAttackEnd(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        (react as IGhostReactor).OnAttackEnd();
        if (map.IsForwardMovable || IsBackwardMovable || IsRightMovable || IsLeftMovable)
        {
            (react as IGhostReactor).OnAppear();
        }
        return base.Execute();
    }
}

public class GhostAttackLeave : FlyingAttackLeave
{
    public GhostAttackLeave(EnemyCommandTarget target, float duration) : base(target, duration) { }
    protected override bool Action()
    {
        // completeTween = tweenMove.DelayedCall(0.1f, (react as GhostReactor).OnAttackEnd).Play();
        return base.Action();
    }
}
