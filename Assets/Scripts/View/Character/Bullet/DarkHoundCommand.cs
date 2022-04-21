using UniRx;
using System;
using DG.Tweening;

public class DarkHoundMove : BulletMove
{
    protected override Tween MoveForward()
    {
        return DOTween.Sequence()
            .Join(tweenMove.MoveForward(TILE_UNIT * 0.4f))
            .Join(tweenMove.Rotate((react as DarkHoundReactor).TargetAngle * 0.6f))
            .SetUpdate(false)
            .Play();
    }

    public DarkHoundMove(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = MoveForward();
        completeTween = AttackSequence.Play();
        validateTween = ValidateTween().Play();

        (react as IBulletReactor).ReduceHP();

        return ObservableComplete();
    }
}
