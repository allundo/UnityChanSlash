using UniRx;
using System;
using DG.Tweening;

public class WitchCommand : EnemyTurnCommand
{
    protected WitchAnimator witchAnim;
    protected WitchReactor witchReact;

    public WitchCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        witchAnim = target.anim as WitchAnimator;
        witchReact = target.react as WitchReactor;
    }
}

public class WitchTargetAttack : WitchCommand
{
    protected IAttack targetAttack;
    protected IAttack targetCritical;

    public WitchTargetAttack(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        targetAttack = target.enemyAttack[1];
        targetCritical = target.enemyAttack[2];
    }

    protected override bool Action()
    {
        witchReact.OnAppear();
        witchAnim.targetAttack.Fire();
        playingTween = DOTween.Sequence()
            .Join(targetAttack.AttackSequence(duration))
            .Join(targetCritical.AttackSequence(duration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchBackStep : WitchCommand
{
    public WitchBackStep(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!map.IsBackwardMovable) return false;

        witchReact.OnAppear();
        witchAnim.backStep.Fire();

        Pos destPos = map.GetBackward;

        enemyMap.MoveObjectOn(destPos);

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(destPos, 1f, 1f))
            .AppendInterval(0.51f * duration)
            .AppendCallback(() => enemyMap.MoveOnEnemy())
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
        if (!base.Action())
        {
            // Cancel attack
            input.ValidateInput();
            return Observable.Empty(Unit.Default);
        }

        input.Interrupt(doubleAttack, false);
        return ObservableComplete();
    }
}

public class WitchTripleFire : WitchCommand
{
    protected IAttack fire;
    public WitchTripleFire(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        fire = target.magic.launcher[BulletType.FireBall];
    }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 10f;
        float fireDuration = duration - interval * 2;

        witchReact.OnAppear();
        witchAnim.fire.Fire();

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
    protected IAttack ice;
    public WitchDoubleIce(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        ice = target.magic.launcher[BulletType.IceBullet];
    }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 15f;
        float fireDuration = duration - interval;

        witchReact.OnAppear();
        witchAnim.fire.Fire();

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

        input.Interrupt(doubleAttackStart, false);
        return ObservableComplete();
    }
}