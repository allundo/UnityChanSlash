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
    protected MobReactor react;
    protected MobInput input;
    protected MapUtil map;

    protected IObserver<bool> onValidateInput;

    public Command(CommandTarget target, float duration, float validateTiming = 0.5f)
    {
        this.target = target;

        this.duration = duration * DURATION_UNIT;
        this.invalidDuration = this.duration * validateTiming;
        tweenMove = new TweenMove(target.transform, this.duration);

        anim = target.anim;
        react = target.react;
        input = target.input;
        map = target.map;
    }

    protected Tween playingTween = null;
    protected Tween validateTween = null;
    protected List<Action> onCompleted = new List<Action>();

    public virtual void Cancel()
    {
        playingTween?.Kill();
        validateTween?.Kill();
        input.ValidateInput();
        onCompleted.Clear();
    }

    public virtual IObservable<Unit> Execute()
    {
        validateTween = ValidateTween().Play();

        if (Action())
        {
            return ExecOnCompleted(() => playingTween?.Complete());
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
        return DOTweenCompleted(duration, Unit.Default).IgnoreElements();
    }

    /// <summary>
    /// Since Observable.Timer() has about 25 frames delay on complete compaired to DOVirtual.DelayedCall(), <br />
    /// you should use this DelayedCall based Observable to work with DOTween durations.
    /// /// </summary>
    /// <param name="dueTimeSec">wait time second for notification</param>
    /// <param name="value">will be notified by OnNext()</param>
    protected IObservable<T> DOTweenCompleted<T>(float dueTimeSec, T value, bool ignoreTimeScale = false)
    {
        return DOVirtual.DelayedCall(dueTimeSec, DoOnCompleted, ignoreTimeScale).OnCompleteAsObservable(value);
    }

    protected Tween DOTweenTimer(float dueTimeSec, TweenCallback callback, bool ignoreTimeScale = false)
        => DOVirtual.DelayedCall(dueTimeSec, callback, ignoreTimeScale);

    protected void DoOnCompleted()
    {
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
        map.ResetOnCharacter();
        anim.die.Fire();

        return ExecOnCompleted(() => react.FadeOutToDead()); // Don't validate input.
    }
}
