using UniRx;
using System;
using DG.Tweening;

public class WitchCommand : EnemyTurnCommand
{
    protected WitchAnimator witchAnim;
    protected WitchReactor witchReact;
    protected MagicAndDouble magicAndDouble;

    public WitchCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        witchAnim = target.anim as WitchAnimator;
        witchReact = target.react as WitchReactor;
        magicAndDouble = target.magic as MagicAndDouble;
    }
}

public class WitchTargetAttack : WitchCommand
{
    protected IAttack targetAttack;

    public WitchTargetAttack(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        targetAttack = target.enemyAttack[1];
    }

    protected override bool Action()
    {
        if (!witchReact.OnAppear()) return false;

        witchAnim.targetAttack.Fire();
        playingTween = targetAttack.AttackSequence(duration).Play();

        return true;
    }
}

public class WitchJumpOver : WitchCommand
{
    public WitchJumpOver(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsJumpable) return false;

        witchReact.OnAppear();
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

    public WitchJumpOverAttack(EnemyCommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
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
    public WitchBackStep(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsBackwardMovable) return false;

        witchReact.OnAppear();
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

    public WitchBackStepAttack(EnemyCommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
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
    public WitchTripleFire(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 10f;
        float fireDuration = duration - interval * 2;

        witchReact.OnAppear();
        witchAnim.fire.Fire();

        ILauncher fire = target.magic.launcher[BulletType.FireBall];
        playingTween = DOTween.Sequence()
            .Join(fire.AttackSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.AttackSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.AttackSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchDoubleIce : WitchCommand
{
    public WitchDoubleIce(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 15f;
        float fireDuration = duration - interval;

        witchReact.OnAppear();
        witchAnim.fire.Fire();

        ILauncher ice = target.magic.launcher[BulletType.IceBullet];

        playingTween = DOTween.Sequence()
            .Join(ice.AttackSequence(fireDuration))
            .AppendInterval(interval)
            .Join(ice.AttackSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchMagic : WitchCommand
{
    public WitchMagic(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        // TODO: Implement command action
        witchReact.OnAppear();
        witchAnim.magic.Fire();
        return true;
    }
}

public class WitchDoubleAttackLaunch : FlyingAttack
{
    protected ICommand doubleAttackStart;
    public WitchDoubleAttackLaunch(EnemyCommandTarget target, float duration) : base(target, duration)
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
