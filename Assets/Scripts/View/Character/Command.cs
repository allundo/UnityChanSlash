using DG.Tweening;
using System;
using System.Collections.Generic;
using UniRx;

public abstract class Command
{
    public static float FRAME_UNIT = 0.01666667f;
    protected const float TILE_UNIT = 2.5f;

    protected float duration;
    protected float invalidDuration;

    protected TweenMove tweenMove;

    protected CommandTarget target;
    protected MobAnimator anim;
    protected MobReactor react;
    protected MobInput input;
    protected MapUtil map;

    protected IObserver<bool> onValidateInput;

    /// <summary>
    /// Initializes Command infomation.
    /// </summary>
    /// <param name="target">Target GameObject to apply Command execution</param>
    /// <param name="duration">Command duration time with frame unit</param>
    /// <param name="validateTiming">Normalized input validating timing of command duration</param>
    public Command(CommandTarget target, float duration, float validateTiming = 0.5f)
    {
        this.target = target;

        this.duration = duration * FRAME_UNIT;
        this.invalidDuration = this.duration * validateTiming;
        tweenMove = new TweenMove(target.transform, target.map, this.duration);

        anim = target.anim;
        react = target.react;
        input = target.input;
        map = target.map;
    }

    protected Tween playingTween = null;
    protected Tween completeTween = null;
    protected Tween validateTween = null;
    protected List<Action> onCompleted = new List<Action>();

    public virtual void Cancel()
    {
        playingTween?.Kill();
        completeTween?.Complete(true);
        validateTween?.Kill();
        input.ValidateInput();
        onCompleted.Clear();
    }

    ///  <summary>
    /// Cancels validating input on current Command execution. <br />
    /// Make sure that a Command including input validation like validateTween is queued next. <br />
    /// </summary>
    public virtual void CancelValidate()
    {
        validateTween?.Kill();
    }

    public virtual IObservable<Unit> Execute()
    {
        validateTween = ValidateTween().Play();

        if (Action())
        {
            return ObservableComplete();
        }
        else
        {
            Cancel();
            return Observable.Empty<Unit>();
        }
    }

    protected virtual Tween ValidateTween()
    {
        return DOTweenTimer(invalidDuration, () => input.ValidateInput());
    }

    protected virtual IObservable<Unit> ExecOnCompleted(params Action[] onCompleted)
    {
        onCompleted.ForEach(action => SetOnCompleted(action));
        return ObservableComplete();
    }

    protected IObservable<Unit> ObservableComplete(float timeScale = 1f)
        => DOTweenCompleted(duration * timeScale, Unit.Default);

    /// <summary>
    /// Since Observable.Timer() has about 25 frames delay on complete compaired to DOVirtual.DelayedCall(), <br />
    /// you should use this DelayedCall based Observable to work with DOTween durations.
    /// /// </summary>
    /// <param name="dueTimeSec">wait time second for notification</param>
    /// <param name="value">will be notified by OnNext()</param>
    protected IObservable<T> DOTweenCompleted<T>(float dueTimeSec, T value, bool ignoreTimeScale = false)
    {
        return DOTweenTimer(dueTimeSec, DoOnCompleted, ignoreTimeScale)
            .OnCompleteAsObservable(value)
            .IgnoreElements();
    }

    protected Tween DOTweenTimer(float dueTimeSec, TweenCallback callback, bool ignoreTimeScale = false)
        => DOVirtual.DelayedCall(dueTimeSec, callback, ignoreTimeScale);

    protected void DoOnCompleted()
    {
        playingTween?.Complete(true);
        completeTween?.Complete(true);

        onCompleted.ForEach(action => action());
        onCompleted.Clear();
    }

    protected virtual bool Action() => false;

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

    protected void SetOnCompleted(Action action)
    {
        this.onCompleted.Add(action);
    }
}

public class DieCommand : Command
{
    public DieCommand(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        map.RemoveObjectOn();
        react.OnDie();

        return ExecOnCompleted(() => react.FadeOutOnDead()); // Don't validate input.
    }
}
