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

    protected float startSec;
    protected float finishSec;

    protected ISubject<IReactor> hitSubject = new Subject<IReactor>();
    public IObservable<IReactor> Hit => hitSubject;

    void Start()
    {
        startSec = FrameToSec(startFrame);
        finishSec = FrameToSec(finishFrame);
    }

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

    protected override void OnHitFinished()
    {
        // Make sure to keep hit detection at least 1 frame.
        // Especially for speed up mode such as player resting.
        Observable.NextFrame().Subscribe(_ => attackCollider.enabled = false).AddTo(this);
    }


    public override Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .InsertCallback(startSec, OnHitStart)
            .InsertCallback(finishSec, OnHitFinished)
            .SetUpdate(false);
    }
}
