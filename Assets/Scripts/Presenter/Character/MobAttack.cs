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
    /// <summary>
    /// Returns critical attack sequence as a Tween
    /// </summary>
    /// <param name="additionalSpeed">Speed added to normal motion speed multiplier.</param>
    /// <param name="criticalMultiplier">Multiplier applied to normal attack power.</param>
    /// <param name="expandScale">Expand scale applied to normal attack collider.</param>
    /// <returns></returns>
    Tween CriticalAttackSequence(int additionalSpeed = 1, float criticalMultiplier = 2.5f, float expandScale = 1.5f);
}

[RequireComponent(typeof(BoxCollider))]
public class MobAttack : Attack, IAttackHitDetect, IMobAttack
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
    [SerializeField] protected int speed = 1;

    /// <summary>
    /// Motion frame rate of animation clip
    /// </summary>
    [SerializeField] protected int frameRate = 30;

    protected float startSec;
    protected float finishSec;

    protected ISubject<IReactor> hitSubject = new Subject<IReactor>();
    public IObservable<IReactor> Hit => hitSubject;

    protected BoxCollider boxCollider;
    protected Vector3 defaultSize;
    protected float defaultAttack;

    void Start()
    {
        boxCollider = attackCollider as BoxCollider;
        defaultSize = boxCollider.size;
        defaultAttack = attackMultiplier;

        startSec = FrameToSec(startFrame);
        finishSec = FrameToSec(finishFrame);
    }

    private float FrameToSec(int frame) => FrameToSec(frame, this.speed);

    private float FrameToSec(int frame, int speed)
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

    public virtual Tween CriticalAttackSequence(int additionalSpeed = 1, float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => CriticalGain(criticalMultiplier, expandScale))
            .InsertCallback(FrameToSec(startFrame, speed + additionalSpeed), OnHitStart)
            .InsertCallback(FrameToSec(finishFrame, speed + additionalSpeed), OnHitFinished)
            .OnComplete(ResetAttackPower)
            .SetUpdate(false);
    }

    protected void CriticalGain(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        boxCollider.size *= expandScale;
        attackMultiplier *= criticalMultiplier;
    }

    protected void ResetAttackPower()
    {
        boxCollider.size = defaultSize;
        attackMultiplier = defaultAttack;
    }
}
