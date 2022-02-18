using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class WitchDoubleCommand : BulletCommand
{
    protected WitchDoubleAnimator witchAnim;
    protected BulletReactor bulletReact;
    protected float attackTimeScale = 0.75f;
    protected float decentVec = -0.5f;

    public WitchDoubleCommand(BulletCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        witchAnim = target.anim as WitchDoubleAnimator;
        bulletReact = target.react as BulletReactor;
    }
}

public class WitchDoubleJump : WitchDoubleCommand
{
    protected ICommand doubleAttack;

    public WitchDoubleJump(BulletCommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        witchAnim.jumpOver.Fire();

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(map.GetJump, 1f, 1.6f))
            .Join(tweenMove.TurnLB.SetEase(Ease.Linear))
            .SetUpdate(false)
            .Play();

        map.TurnBack();

        input.Interrupt(doubleAttack, false);
        return ObservableComplete();
    }
}

public class WitchDoubleBackStep : WitchDoubleCommand
{
    protected ICommand doubleAttack;

    public WitchDoubleBackStep(BulletCommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        witchAnim.backStep.Fire();

        playingTween = tweenMove.Jump(map.GetBackward, 1f, 1f).Play();

        input.Interrupt(doubleAttack, false);
        return ObservableComplete();
    }
}

public class WitchDoubleLaunch : WitchDoubleCommand
{
    protected ICommand doubleAttackStart;
    public WitchDoubleLaunch(BulletCommandTarget target, float duration) : base(target, duration)
    {
        doubleAttackStart = new WitchDoubleStart(target, duration * 0.5f);
    }

    public override IObservable<Unit> Execute()
    {
        input.Interrupt(doubleAttackStart, false);
        return ObservableComplete();
    }
}

public class WitchDoubleStart : WitchDoubleCommand
{
    protected ICommand attackKeep;

    public WitchDoubleStart(BulletCommandTarget target, float duration) : base(target, duration)
    {
        attackKeep = new WitchDoubleKeep(target, duration);
    }

    public override IObservable<Unit> Execute()
    {

        Vector3 dest = map.CurrentVec3Pos + (map.GetForwardVector() + new Vector3(0f, decentVec, 0f)) * attackTimeScale;

        map.MoveObjectOn(map.GetForward);

        playingTween = tweenMove.Move(dest, attackTimeScale).Play();

        completeTween = attack.AttackSequence(duration).Play();

        target.input.Interrupt(attackKeep, false);

        return ObservableComplete(attackTimeScale);
    }
}
public class WitchDoubleKeep : WitchDoubleCommand
{
    protected ICommand attackEnd;

    public WitchDoubleKeep(BulletCommandTarget target, float duration) : base(target, duration)
    {
        attackEnd = new WitchDoubleEnd(target, duration);
    }

    public override IObservable<Unit> Execute()
    {
        Vector3 dest = map.CurrentVec3Pos + map.GetForwardVector();

        map.MoveObjectOn(map.GetForward);

        playingTween = tweenMove.Move(dest).Play();

        completeTween = attack.AttackSequence(duration).Play();

        target.input.Interrupt(attackEnd, false);

        return ObservableComplete();
    }
}

public class WitchDoubleEnd : WitchDoubleCommand
{
    protected ICommand leave;
    public WitchDoubleEnd(BulletCommandTarget target, float duration) : base(target, duration)
    {
        leave = new WitchDoubleLeave(target, duration / (1f - attackTimeScale) * 2f);
    }
    public override IObservable<Unit> Execute()
    {
        Vector3 destTileVec = map.DestVec;
        Vector3 dest = map.CurrentVec3Pos + new Vector3(destTileVec.x, decentVec * (1f - attackTimeScale), destTileVec.z);

        playingTween = tweenMove.Move(dest).Play();
        input.Interrupt(leave, false);

        return ObservableComplete();
    }
}

public class WitchDoubleLeave : WitchDoubleCommand
{
    public WitchDoubleLeave(BulletCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = tweenMove.Move(map.onTilePos, 1f, Ease.OutQuad).Play();
        react.OnDie();

        return ObservableComplete();
    }
}