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
        onCompleted.Clear();
    }

    public virtual IObservable<bool> Execute()
    {
        Action();

        return ExecObservable();
    }

    protected virtual IObservable<bool> ExecObservable()
    {
        var onValidate = DOTweenTimer(invalidDuration, false);

        return Observable.Merge(onValidate, ExecOnCompleted(() => playingTween?.Complete()));
    }

    protected virtual IObservable<bool> ExecOnCompleted(params Action[] onCompleted)
    {
        var onCompleteObservable = DOTweenTimer(duration, false);

        onCompleted.ForEach(action => SetOnCompleted(action));

        onCompleteDisposable = onCompleteObservable
            .Subscribe(_ => this.onCompleted.ForEach(action => action()))
            .AddTo(target);

        return onCompleteObservable.IgnoreElements();
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
    /// Notify onDead event.
    /// </summary>
    protected void NotifyOnDead() => target.onDead.OnNext(Unit.Default);

    protected void SetOnCompleted(Action action)
    {
        this.onCompleted.Add(action);
    }
}

public class DieCommand : Command
{
    public DieCommand(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<bool> Execute()
    {
        map.ResetOnCharacter();
        anim.die.Fire();

        return ExecOnCompleted(NotifyOnDead); // Don't validate input.

    }
}
