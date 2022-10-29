using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public interface IAttackHitDetect : IAttack
{
    IObservable<IReactor> Hit { get; }
}

public interface IMobAttack : IAttack
{
    void OnDie();
}

[RequireComponent(typeof(BoxCollider))]
public class MobAttack : Attack, IMobAttack, IAttackHitDetect
{
    /// <summary>
    /// Attack hit starting frame on animation clip
    /// </summary>
    [SerializeField] protected int startFrame = 0;

    /// <summary>
    /// Attack hit finishing frame on animation clip
    /// </summary>
    [SerializeField] protected int finishFrame = 0;

    /// <summary>
    /// Animation speed set on Mecanim Animator
    /// </summary>
    [SerializeField] protected float speed = 1;

    /// <summary>
    /// Motion frame rate of animation clip
    /// </summary>
    [SerializeField] protected float frameRate = 30;

    protected float startSec;
    protected float finishSec;

    protected ISubject<IReactor> hitSubject = new Subject<IReactor>();
    public IObservable<IReactor> Hit => hitSubject;

    protected BoxCollider boxCollider;
    protected Vector3 defaultSize;
    protected float defaultAttack;

    protected virtual void Start()
    {
        boxCollider = attackCollider as BoxCollider;
        defaultSize = boxCollider.size;
        defaultAttack = attackMultiplier;

        startSec = FrameToSec(startFrame);
        finishSec = FrameToSec(finishFrame);
    }

    protected virtual float FrameToSec(int frame) => FrameToSec(frame, this.speed);

    protected float FrameToSec(int frame, float speed)
    {
        return (float)frame / frameRate / speed;
    }

    public virtual void OnDie() { }

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

    public override Sequence AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .InsertCallback(startSec, OnHitStart)
            .InsertCallback(finishSec, OnHitFinished)
            .SetUpdate(false);
    }

    protected void ResetAttackPower()
    {
        boxCollider.size = defaultSize;
        attackMultiplier = defaultAttack;
    }
}
