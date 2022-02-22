using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public interface IAttackHitDetect : IAttack
{
    IObservable<IReactor> Hit { get; }
}

[RequireComponent(typeof(Collider))]
public class MobAttack : Attack, IAttackHitDetect
{
    [SerializeField] protected int startFrame = 0;
    [SerializeField] protected int finishFrame = 0;
    [SerializeField] protected int speed = 1;
    [SerializeField] protected int frameRate = 30;

    protected ISubject<IReactor> hitSubject = new Subject<IReactor>();
    public IObservable<IReactor> Hit => hitSubject;

    private float FrameToSec(int frame)
    {
        return (float)frame / (float)frameRate / (float)speed;
    }

    protected override IReactor OnHitAttack(Collider collider)
    {
        var target = base.OnHitAttack(collider);
        if (target != null) hitSubject.OnNext(target);
        return target;
    }

    public override Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .Join(DOVirtual.DelayedCall(FrameToSec(startFrame), OnHitStart))
            .Join(DOVirtual.DelayedCall(FrameToSec(finishFrame), OnHitFinished))
            .SetUpdate(false);
    }
}
