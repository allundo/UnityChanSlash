using UniRx;
using System;

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

public class GhostAttackStart : FlyingAttackStart
{
    protected override bool IsForwardMovable => map.ForwardTile.IsViewOpen;

    protected override FlyingAttackEnd AttackEndCommand(EnemyCommandTarget target, float duration)
        => new GhostAttackEnd(target, duration);

    public GhostAttackStart(EnemyCommandTarget target, float duration) : base(target, duration, -0.1f) { }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        (react as GhostReactor).OnHide();
        return base.Execute();
    }
}

public class GhostAttackEnd : FlyingAttackEnd
{
    protected override bool IsForwardMovable => map.IsForwardMovable;
    protected override bool IsBackwardMovable => map.IsBackwardMovable;
    protected override bool IsRightMovable => map.IsRightMovable;
    protected override bool IsLeftMovable => map.IsLeftMovable;

    public GhostAttackEnd(EnemyCommandTarget target, float duration) : base(target, duration, -0.1f) { }

    public override IObservable<Unit> Execute()
    {
        if (IsForwardMovable || IsBackwardMovable || IsRightMovable || IsLeftMovable)
        {
            (react as GhostReactor).OnAppear();
        }
        return base.Execute();
    }
}
