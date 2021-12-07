using UniRx;
using System;
using DG.Tweening;

public abstract class BulletCommand : Command
{
    public BulletCommand(BulletCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming) { }

    protected bool IsMovable => map.ForwardTile.IsViewOpen;

    protected virtual Tween MoveForward()
    {
        var destPos = map.WorldPos(map.SetOnCharacter(map.GetForward, false));
        return tweenMove.GetLinearMove(destPos);
    }

    protected virtual Tween MoveForward(float ratio, bool isSpeedConstant = true)
    {
        if (ratio > 0.5f) map.SetOnCharacter(map.GetForward, false);

        if (isSpeedConstant)
        {
            duration *= ratio;
            invalidDuration *= ratio;
        }

        var destPos = map.CurrentVec3Pos + map.dir.LookAt * TILE_UNIT * ratio;
        return tweenMove.GetLinearMove(destPos);
    }
}

public class BulletFire : BulletCommand
{
    public BulletFire(BulletCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (IsMovable)
        {
            playingTween = MoveForward().Play();
        }
        else
        {
            playingTween = MoveForward(0.75f).Play();
            tweenMove.SetDelayedCall(1.0f, target.input.InputDie).Play();
        }

        return true;
    }
}

public class BulletMove : BulletCommand
{
    protected IAttack attack;

    public BulletMove(BulletCommandTarget target, float duration) : base(target, duration)
    {
        attack = target.attack;
    }

    protected override bool Action()
    {
        if (IsMovable)
        {
            playingTween = MoveForward().Play();
            completeTween = attack.AttackSequence(duration).Play();
        }
        else
        {
            playingTween = MoveForward(0.75f).Play();
            tweenMove.SetDelayedCall(1.0f, target.input.InputDie).Play();
        }

        return true;
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