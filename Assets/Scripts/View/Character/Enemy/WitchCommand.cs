using UniRx;
using System;
using DG.Tweening;

public class WitchCommand : EnemyTurnCommand
{
    protected WitchAnimator witchAnim;
    protected WitchReactor witchReact;
    protected MagicAndDouble magicAndDouble;

    public WitchCommand(ICommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        witchAnim = target.anim as WitchAnimator;
        witchReact = target.react as WitchReactor;
        magicAndDouble = target.magic as MagicAndDouble;
    }
}

public class WitchTargetAttack : WitchCommand
{
    protected IAttack targetAttack;

    public WitchTargetAttack(ICommandTarget target, float duration) : base(target, duration)
    {
        targetAttack = target.Attack(1);
    }

    protected override bool Action()
    {
        if (!witchReact.Appear()) return false;

        witchAnim.targetAttack.Fire();
        completeTween = targetAttack.AttackSequence(duration).Play();

        return true;
    }
}

public class WitchJumpOver : WitchCommand
{
    public WitchJumpOver(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsJumpable) return false;

        witchReact.Appear();
        witchAnim.jump.Fire();

        Pos destPos = enemyMap.GetJump;

        enemyMap.MoveObjectOn(destPos);
        enemyMap.TurnBack();

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(destPos, 1f, 1.6f))
            .Join(tweenMove.TurnLB.SetEase(Ease.Linear))
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.8f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        return true;
    }
}
public class WitchJumpOverAttack : WitchJumpOver
{
    protected ICommand doubleAttack;

    public WitchJumpOverAttack(ICommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        if (!enemyMap.IsJumpable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        magicAndDouble.backStepWitchLauncher.Fire();
        base.Action();

        input.Interrupt(doubleAttack, false);
        return ObservableComplete();
    }
}
public class WitchBackStep : WitchCommand
{
    public WitchBackStep(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsBackwardMovable) return false;

        witchReact.Appear();
        witchAnim.backStep.Fire();

        Pos destPos = map.GetBackward;

        enemyMap.MoveObjectOn(destPos);

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(destPos, 1f, 1f))
            .InsertCallback(0.51f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchBackStepAttack : WitchBackStep
{
    protected ICommand doubleAttack;

    public WitchBackStepAttack(ICommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        if (!enemyMap.IsBackwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        magicAndDouble.jumpOverWitchLauncher.Fire();
        base.Action();

        input.Interrupt(doubleAttack, false);
        return ObservableComplete();
    }
}

public class WitchTripleFire : WitchCommand
{
    public WitchTripleFire(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 10f;
        float fireDuration = duration - interval * 2;

        witchReact.Appear();
        witchAnim.fire.Fire();

        ILauncher fire = target.magic.launcher[MagicType.FireBall];
        playingTween = DOTween.Sequence()
            .Join(fire.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.FireSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchDoubleIce : WitchCommand
{
    public WitchDoubleIce(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 15f;
        float fireDuration = duration - interval;

        witchReact.Appear();
        witchAnim.fire.Fire();

        ILauncher ice = target.magic.launcher[MagicType.IceBullet];

        playingTween = DOTween.Sequence()
            .Join(ice.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(ice.FireSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchLaser : WitchCommand
{
    public WitchLaser(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        witchReact.Appear();
        witchReact.OnLaserStart();
        witchAnim.magic.Fire();

        ILauncher laser = target.magic.launcher[MagicType.LightLaser];
        playingTween = laser.FireSequence(2f).Play();

        return true;
    }
}

public class WitchSummonMonster : WitchCommand
{
    public WitchSummonMonster(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (witchReact.IsSummoning) return false;

        witchReact.Appear();
        witchReact.OnSummonStart();
        witchAnim.summon.Fire();
        playingTween = tweenMove.DelayedCall(0.8f, witchReact.Summon).Play();
        return true;
    }
}

public class WitchDoubleAttackLaunch : FlyingAttack
{
    protected ICommand doubleAttackStart;
    public WitchDoubleAttackLaunch(ICommandTarget target, float duration) : base(target, duration)
    {
        doubleAttackStart = new GhostAttackStart(target, duration * 0.5f);
    }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        (anim as WitchAnimator).attackStart.Fire();
        input.Interrupt(doubleAttackStart, false);
        return ObservableComplete();
    }
}
