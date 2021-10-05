using DG.Tweening;
using System;
using System.Collections.Generic;
using UniRx;

public abstract class Command
{
    public static float DURATION_UNIT = 0.6f;
    protected const float TILE_UNIT = 2.5f;

    protected float duration;
    protected float invalidDuration;

    protected TweenMove tweenMove;

    protected CommandTarget target;
    protected MobAnimator anim;
    protected MapUtil map;

    protected IObserver<bool> onValidateInput;

    public Command(CommandTarget target, float duration, float validateTiming = 0.5f)
    {
        this.target = target;

        this.duration = duration * DURATION_UNIT;
        this.invalidDuration = this.duration * validateTiming;
        tweenMove = new TweenMove(target.transform, this.duration);

        anim = target.anim;
        map = target.map;
    }

    protected Tween playingTween = null;
    protected Tween validateTween = null;
    protected IDisposable onCompleteDisposable = null;
    protected List<Action> onCompleted = new List<Action>();

    public virtual void Cancel()
    {
        playingTween?.Kill();
        onCompleteDisposable?.Dispose();
    }

    public virtual IObservable<bool> Execute()
    {
        var onValidate = DOTweenTimer(invalidDuration, false);

        var onCompleted = DOTweenTimer(duration, false);

        SetOnCompleted(() => playingTween?.Complete());

        onCompleteDisposable = onCompleted
            .Subscribe(_ => this.onCompleted.ForEach(action => action()))
            .AddTo(target);

        return Execute(Observable.Merge(onValidate, onCompleted.IgnoreElements()));
    }

    protected virtual IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        Action();
        return execObservable;
    }

    /// <summary>
    /// Since Observable.Timer() has about 25 frames delay on complete compaired to DOVirtual.DelayedCall(), <br />
    /// you should use this DelayedCall based Observable to work with DOTween durations.
    /// /// </summary>
    /// <param name="dueTimeSec">wait time second for notification</param>
    /// <param name="value">will be notified by OnNext()</param>
    protected IObservable<T> DOTweenTimer<T>(float dueTimeSec, T value, bool ignoreTimeScale = false)
    {
        return DOVirtual.DelayedCall(dueTimeSec, null, ignoreTimeScale).OnCompleteAsObservable(value);
    }

    protected virtual void Action() { }

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
    /// Reserve onDead notification at the end of Command execution
    /// </summary>
    protected void NotifyOnDeadFinal()
        => SetOnCompleted(() => target.onDead.OnNext(Unit.Default));

    protected void SetOnCompleted(Action action)
    {
        this.onCompleted.Add(action);
    }
}

public class DieCommand : Command
{
    public DieCommand(CommandTarget target, float duration) : base(target, duration) { }

    protected override IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        map.ResetOnCharacter();
        anim.die.Fire();

        NotifyOnDeadFinal();

        return null; // Don't validate input and dispatch next Command.
    }
}
