using UniRx;
using System;
using DG.Tweening;

public abstract class BulletCommand : Command
{
    protected IAttack attack;
    public BulletCommand(BulletCommandTarget target, float duration, float validateTiming = 1f) : base(target, duration, validateTiming)
    {
        attack = target.attack;
    }

    protected bool IsMovable => map.ForwardTile.IsViewOpen;

    protected virtual Tween MoveForward()
    {
        var destPos = map.WorldPos(map.SetOnCharacter(map.GetForward, false));
        return tweenMove.GetLinearMove(destPos);
    }

    protected virtual Tween MoveForward(float ratio, bool isSpeedConstant = true)
    {
        if (ratio > 0.5f) map.SetOnCharacter(map.GetForward, false);
        return tweenMove.GetLinearMove(map.dir.LookAt * TILE_UNIT * ratio, ratio).SetRelative();
    }
}

public class BulletFire : BulletCommand
{
    public BulletFire(BulletCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        if (IsMovable)
        {
            playingTween = MoveForward().Play();
            completeTween = attack.AttackSequence(duration * 0.5f).SetDelay(duration * 0.5f).Play();
            validateTween = ValidateTween().Play();
            return ObservableComplete();
        }
        else
        {
            playingTween = MoveForward(0.75f).Play();

            // Call InputDie() independently because it cancels OnComplete Actions during executing them.
            tweenMove.SetDelayedCall(0.75f, target.input.InputDie).Play();
            return ObservableComplete(0.75f);
        }
    }
}

public class BulletMove : BulletCommand
{

    public BulletMove(BulletCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        if (IsMovable)
        {
            playingTween = MoveForward().Play();
            completeTween = attack.AttackSequence(duration).Play();
            validateTween = ValidateTween().Play();
            return ObservableComplete();
        }
        else
        {
            playingTween = MoveForward(0.75f).Play();

            tweenMove.SetDelayedCall(0.75f, target.input.InputDie).Play();
            return ObservableComplete(0.75f);
        }
    }
}

public class BulletDie : BulletCommand
{
    public BulletDie(BulletCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = MoveForward(0.1f).Play();
        react.FadeOutToDead(0.25f);

        return ExecOnCompleted(() => playingTween?.Complete()); // Don't validate input.
    }
}