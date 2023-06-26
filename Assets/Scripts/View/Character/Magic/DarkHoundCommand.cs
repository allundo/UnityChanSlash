using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class DarkHoundMove : MagicMove
{
    protected override Tween MoveForward()
    {
        float targetAngle = (react as DarkHoundReactor).TargetAngle;
        float rotateAngle = (targetAngle > 0f ? 1 : -1) * Mathf.Min(Mathf.Abs(targetAngle), 24f);

        return DOTween.Sequence()
            .Join(tweenMove.MoveForward(TILE_UNIT * 0.4f))
            .Join(tweenMove.Rotate(rotateAngle))
            .SetUpdate(false)
            .Play();
    }

    public DarkHoundMove(CommandTarget target, float duration, ICommand die) : base(target, duration, die) { }

    public override IObservable<Unit> Execute()
    {
        playingTween = MoveForward();
        completeTween = AttackSequence.Play();
        validateTween = ValidateTween().Play();

        (react as IMortalReactor).ReduceHP();

        return ObservableComplete();
    }
}

public class DarkHoundDie : MagicDie
{
    public DarkHoundDie(CommandTarget target, float duration) : base(target, duration) { }
    protected override Tween MoveForward() => tweenMove.MoveForward(TILE_UNIT * 0.05f).Play();
}
