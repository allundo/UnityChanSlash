using UniRx;
using System;
using DG.Tweening;

public abstract class MagicCommand : Command
{
    protected IAttack attack;
    public MagicCommand(CommandTarget target, float duration, float validateTiming = 1f) : base(target, duration, validateTiming)
    {
        attack = target.Attack(0);
    }

    protected virtual Tween MoveForward() => tweenMove.Linear(map.GetForward);

    protected virtual Tween MoveForward(float ratio, bool isSpeedConstant = true)
    {
        return tweenMove.Linear(map.CurrentVec3Pos + map.GetForwardVector(ratio), isSpeedConstant ? ratio : 1f);
    }
}

public class MagicMove : MagicCommand
{
    protected ICommand die;

    public MagicMove(CommandTarget target, float duration, ICommand die) : base(target, duration)
    {
        this.die = die;
    }

    protected virtual Tween AttackSequence => attack.AttackSequence(duration);

    public override IObservable<Unit> Execute()
    {
        // Forward movable?
        if (map.ForwardTile.IsViewOpen)
        {
            playingTween = MoveForward();
            completeTween = AttackSequence.Play();
            validateTween = ValidateTween().Play();
            return ObservableComplete();
        }
        else
        {
            playingTween = MoveForward(0.75f);

            // Call InputDie() independently because it cancels OnComplete Actions during executing them.
            // FIXME: currently handling it as validate tween to make it cancelable
            validateTween = tweenMove.DelayedCall(0.75f, () => target.interrupt.OnNext(Data(die, true, true))).Play();
            return ObservableComplete(0.75f);
        }
    }
}

public class MagicFire : MagicMove
{
    public MagicFire(CommandTarget target, float duration, ICommand die) : base(target, duration, die) { }

    // Enable attack collider after a half duration
    protected override Tween AttackSequence => attack.AttackSequence(duration * 0.5f).SetDelay(duration * 0.5f);
}

public class MagicDie : MagicCommand
{
    public MagicDie(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = MoveForward(0.1f);
        react.OnDie();

        return ObservableComplete(); // Don't validate input.
    }
}
