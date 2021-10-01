using DG.Tweening;
using System;
using UniRx;

public abstract class Command
{
    public static float DURATION_UNIT = 0.6f;
    protected const float TILE_UNIT = 2.5f;

    protected float duration;

    protected TweenMove tweenMove;

    protected MobAnimator anim;

    protected IObserver<Unit> onDead;
    protected IObserver<Unit> onCompleted;
    protected IObserver<bool> onValidated;

    public Command(MobCommander commander, float duration)
    {
        this.duration = duration * DURATION_UNIT;
        this.tweenMove = new TweenMove(commander.transform, this.duration);

        onDead = commander.onDead;
        onCompleted = commander.onCompleted;
        onValidated = commander.onValidated;
        anim = commander.anim;
    }

    protected Tween playingTween = null;
    protected Tween validateTween = null;
    protected Tween finallyCall = null;

    public virtual void Cancel()
    {
        playingTween?.Kill();
        validateTween?.Kill();
        finallyCall?.Kill();
    }

    public virtual void CancelValidateTween()
    {
        validateTween?.Kill();
    }

    public abstract void Execute();
    public virtual float Speed => 0.0f;
    public virtual float RSpeed => 0.0f;

    protected Sequence JoinTweens(params Tween[] tweens)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < tweens.Length; i++)
        {
            seq.Join(tweens[i]);
        }

        return seq;
    }

    /// <summary>
    /// Play tween and dispatch next command on complete
    /// </summary>
    /// <param name="tween">Tween (or Sequence) to play</param>
    /// <param name="OnComplete">Additional process on complete</param>
    protected void PlayTween(Tween tween, Action OnComplete = null)
    {
        playingTween = tween;

        tween.OnComplete(DispatchFinally(OnComplete)).Play();
    }

    /// <summary>
    /// Reserve validating of next command input
    /// </summary>
    /// <param name="timing">Validate timing specified by normalized (0.0f,1.0f) command duration</param>
    protected virtual void SetValidateTimer(float timing = 0.5f)
    {
        validateTween = tweenMove.SetDelayedCall(timing, () => onValidated.OnNext(false));
    }

    /// <summary>
    /// Reserve next command dispatching after command duration.
    /// </summary>
    /// <param name="OnComplete">Additional process on complete</param>
    protected void SetDispatchFinal(Action OnComplete = null)
    {
        finallyCall = tweenMove.SetFinallyCall(DispatchFinally(OnComplete));
    }

    /// <summary>
    /// Reserve destory commander object after command duration
    /// </summary>
    protected void SetDestoryFinal()
    {
        tweenMove.SetFinallyCall(() => onDead.OnNext(Unit.Default));
    }

    private TweenCallback DispatchFinally(Action OnComplete = null)
    {
        return () =>
        {
            if (OnComplete != null) OnComplete();
            onCompleted.OnNext(Unit.Default);
        };
    }
}

public class DieCommand : Command
{
    public DieCommand(MobCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        anim.die.Fire();
        SetDestoryFinal();
    }
}
