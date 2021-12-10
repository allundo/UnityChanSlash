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

    protected virtual Tween MoveForward() => tweenMove.GetLinearMove(map.GetForward);

    protected virtual Tween MoveForward(float ratio, bool isSpeedConstant = true)
    {
        if (ratio > 0.5f) map.SetObjectOn(map.GetForward);
        return tweenMove.GetLinearMove(map.CurrentVec3Pos + map.dir.LookAt * TILE_UNIT * ratio, isSpeedConstant ? ratio : 1f);
    }


}

public class BulletMove : BulletCommand
{
    public BulletMove(BulletCommandTarget target, float duration) : base(target, duration) { }

    protected virtual Tween AttackSequence => attack.AttackSequence(duration);

    public override IObservable<Unit> Execute()
    {
        // Forward movable?
        if (map.ForwardTile.IsViewOpen)
        {
            playingTween = MoveForward().Play();
            completeTween = AttackSequence.Play();
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

public class BulletFire : BulletMove
{
    public BulletFire(BulletCommandTarget target, float duration) : base(target, duration) { }

    protected override Tween AttackSequence => attack.AttackSequence(duration * 0.5f).SetDelay(duration * 0.5f);
}

public class BulletDie : BulletCommand
{
    public BulletDie(BulletCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = MoveForward(0.1f).Play();
        react.OnDie();

        return ObservableComplete(); // Don't validate input.
    }
}
