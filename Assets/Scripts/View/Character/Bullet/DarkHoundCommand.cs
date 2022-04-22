using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class DarkHoundMove : BulletMove
{
    protected override Tween MoveForward()
    {
        float targetAngle = (react as DarkHoundReactor).TargetAngle;
        float rotateAngle = (targetAngle > 0f ? 1 : -1) * Mathf.Min(Mathf.Abs(targetAngle), 20f);

        return DOTween.Sequence()
            .Join(tweenMove.MoveForward(TILE_UNIT * 0.4f))
            .Join(tweenMove.Rotate(rotateAngle))
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

public class DarkHoundDie : BulletDie
{
    public DarkHoundDie(CommandTarget target, float duration) : base(target, duration) { }
    protected override Tween MoveForward() => tweenMove.MoveForward(TILE_UNIT * 0.05f).Play();
}
