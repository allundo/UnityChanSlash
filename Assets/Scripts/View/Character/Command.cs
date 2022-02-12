using DG.Tweening;
using System;
using System.Collections.Generic;
using UniRx;

public interface ICommand
{
    /// <summary>
    /// Cancel all behavior of the Command.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Cancels validating input on current Command execution. <br />
    /// Make sure that a Command including input validation like validateTween is queued next. <br />
    /// </summary>
    void CancelValidate();

    /// <summary>
    /// Execute command mainly to apply something to CommandTarget.
    /// </summary>
    /// <returns>Observable notifies Command completion.</returns>
    IObservable<Unit> Execute();

    int priority { get; }
    bool IsPriorTo(int target);
    bool IsPriorTo(ICommand cmd);
    ICommand GetContinuation();
}

public class Command : ICommand
{
    public static readonly float FRAME_UNIT = Constants.FRAME_SEC_UNIT;
    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    protected float duration;
    protected float invalidDuration;

    protected TweenMove tweenMove;

    protected CommandTarget target;
    protected MobAnimator anim;
    protected IReactor react;
    protected IInput input;
    protected IMapUtil map;

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

    public Command(IInput input, Tween playing, Tween complete, List<Action> onCompleted)
    {
        this.input = input;
        this.playingTween = Pause(playing);
        this.completeTween = Pause(complete);
        this.onCompleted = onCompleted;
        this.duration = playing == null ? 0f : playing.Duration() - playing.fullPosition;
        this.invalidDuration = this.duration * 0.95f;
    }

    private static Tween Pause(Tween src) => src == null || !src.IsActive() ? null : src.Pause();

    public Tween playingTween { get; protected set; } = null;
    protected Tween completeTween = null;
    protected Tween validateTween = null;
    protected List<Action> onCompleted = new List<Action>();

    public ICommand GetContinuation() => new Command(input, playingTween, completeTween, onCompleted);

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
            return ExecOnCompleted();
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

    protected virtual bool Action()
    {
        if (duration == 0f) return false;
        playingTween?.Play();
        completeTween?.Play();
        return true;
    }

    protected virtual float Speed => 0.0f;
    protected virtual float RSpeed => 0.0f;

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

    public virtual int priority => 0;
    public bool IsPriorTo(int target) => priority > target;
    public bool IsPriorTo(ICommand cmd) => IsPriorTo(cmd.priority);
}

public class DieCommand : Command
{
    public DieCommand(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        map.RemoveObjectOn();
        react.OnDie();

        return ExecOnCompleted(() => react.FadeOutToDead()); // Don't validate input.
    }
}

public class IcedCommand : Command
{
    public override int priority => 20;
    public IcedCommand(CommandTarget target, float duration) : base(target, duration, 0.98f) { }

    protected override bool Action()
    {
        anim.Pause();

        completeTween = tweenMove.DelayedCall(1f, anim.Resume).Play();
        SetOnCompleted(() => react.OnMelt());
        return true;
    }
}
