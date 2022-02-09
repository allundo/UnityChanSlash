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
            (react as GhostReactor).OnAppear();
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
        (react as GhostReactor).OnHide();

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
        if (map.IsForwardMovable) (react as GhostReactor).OnAppear();

        playingTween = LinearMove(GetDest);
        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        return true;
    }
}

public class GhostAttackStart : FlyingAttackStart
{
    protected override bool IsForwardMovable => map.ForwardTile.IsViewOpen;

    protected override FlyingAttackEnd AttackEndCommand(EnemyCommandTarget target, float duration)
        => new GhostAttackEnd(target, duration);

    protected override ICommand PlayNext()
        => MapUtil.IsOnPlayer(map.GetForward) ? this : attackEnd;

    protected override float attackTimeScale => 0.75f;
    protected override float decentVec => -0.1f;

    public GhostAttackStart(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        (react as GhostReactor).OnAttackStart();
        return base.Execute();
    }
}

public class GhostAttackEnd : FlyingAttackEnd
{
    protected override bool IsForwardMovable => map.IsForwardMovable;
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
        (react as GhostReactor).OnAttackEnd();
        if (IsForwardMovable || IsBackwardMovable || IsRightMovable || IsLeftMovable)
        {
            (react as GhostReactor).OnAppear();
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
