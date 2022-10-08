using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class GhostForward : EnemyForward
{
    public GhostForward(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (base.Action())
        {
            mobReact.Appear();
            return true;
        }
        return false;
    }
}

public class GhostThrough : EnemyForward
{
    protected ICommand throughEnd;
    protected ICommand attack;

    public override void Cancel()
    {
        base.Cancel();
        (anim as GhostAnimator).wallThrough.Bool = false;
    }

    protected override void SetSpeed()
    {
        base.SetSpeed();
        (anim as GhostAnimator).wallThrough.Bool = true;
    }

    protected override void ResetSpeed()
    {
        base.ResetSpeed();
        (anim as GhostAnimator).wallThrough.Bool = false;
    }

    public GhostThrough(ICommandTarget target, float duration, ICommand attack) : base(target, duration)
    {
        throughEnd = new GhostThroughEnd(target, duration);
        this.attack = attack;
    }

    public override IObservable<Unit> Execute()
    {
        mobReact.Hide();

        playingTween = LinearMove(GetDest);

        input.Interrupt(MapUtil.IsOnPlayer(mobMap.GetForward) ? attack : throughEnd, false);

        return ObservableComplete();
    }
}

public class GhostThroughEnd : EnemyForward
{
    public GhostThroughEnd(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!mobMap.ForwardTile.IsViewOpen) return false;
        if (mobMap.IsForwardMovable) mobReact.Appear();

        playingTween = LinearMove(GetDest);
        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        return true;
    }
}

public class GhostAttackStart : FlyingAttackStart
{
    protected ICommand attackKeep;

    protected override bool IsForwardMovable => mobMap.ForwardTile.IsViewOpen;

    protected override ICommand AttackEndCommand(ICommandTarget target, float duration)
        => attackEnd ?? new GhostAttackEnd(target, duration);

    protected override ICommand PlayNext()
        => MapUtil.IsOnPlayer(mobMap.GetForward) ? attackKeep : attackEnd;

    protected override float attackTimeScale => 0.75f;
    protected override float decentVec => -0.1f;

    public GhostAttackStart(ICommandTarget target, float duration) : base(target, duration)
    {
        attackEnd = AttackEndCommand(target, duration * (1f - attackTimeScale));
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

    protected ICommand PlayNext() => MapUtil.IsOnPlayer(mobMap.GetForward) ? this : attackEnd;

    public GhostAttackKeep(ICommandTarget target, float duration, ICommand attackEnd) : base(target, duration)
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

        Vector3 dest = mobMap.CurrentVec3Pos + map.GetForwardVector();

        mobMap.MoveObjectOn(map.GetForward);

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
    protected override bool IsForwardMovable => mobMap.ForwardTile.IsViewOpen;
    protected override bool IsBackwardMovable => mobMap.IsBackwardMovable;
    protected override bool IsRightMovable => mobMap.IsRightMovable;
    protected override bool IsLeftMovable => mobMap.IsLeftMovable;

    protected override float attackTimeScale => 0.75f;
    protected override float decentVec => -0.1f;

    public GhostAttackEnd(ICommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        (react as IGhostReactor).OnAttackEnd();

        // Apply enemyMap.MoveObjectOn()
        var observableComplete = base.Execute();

        // Appear if destination tile is enterable
        (react as IGhostReactor).Appear();

        return observableComplete;
    }
}
