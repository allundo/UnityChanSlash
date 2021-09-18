using DG.Tweening;
using System.Collections.Generic;

public class SequenceEx
{
    private List<Tween> list = new List<Tween>();

    private SequenceEx prev = null;
    private SequenceEx next = null;

    private float maxDuration = 0f;
    private Tween longestTween = null;
    private TweenCallback onComplete = () => { };
    private TweenCallback callback = () => { };

    public bool IsPlaying() => Playing != null;

    public bool isPlaying = false;

    private bool isIndependentUpdate = DOTween.defaultTimeScaleIndependent;

    public SequenceEx SetUpdate(bool value = false)
    {
        isIndependentUpdate = value;
        return this;
    }

    public SequenceEx SetSkipable(bool isSkipable = true)
    {
        this.isSkipable = isSkipable;
        return this;
    }
    private bool isSkipable = true;

    private SequenceEx Playing => isPlaying ? this : next?.Playing;

    private SequenceEx InitNext()
    {
        next = new SequenceEx().SetUpdate(isIndependentUpdate);
        next.prev = this;

        onComplete = (longestTween.onComplete ?? (() => { })).Clone() as TweenCallback;

        longestTween.OnComplete(() =>
        {
            onComplete();
            PlayNext();
        });

        return next;
    }

    public SequenceEx Join(Tween tween)
    {
        float duration = tween.Duration();

        if (duration > maxDuration)
        {
            longestTween = tween;
            maxDuration = duration;
        }

        list.Add(tween.SetUpdate(isIndependentUpdate));
        return this;
    }

    public SequenceEx Append(Tween tween)
        => (longestTween == null) ? Join(tween) : InitNext().Join(tween);

    public SequenceEx AppendInterval(float duration)
        => Append(DOVirtual.DelayedCall(duration, () => { }, isIndependentUpdate));

    public SequenceEx AppendCallback(TweenCallback callback)
        => InitNext().InsertCallback(callback);

    public SequenceEx InsertCallback(TweenCallback callback)
    {
        TweenCallback tmp = this.callback.Clone() as TweenCallback;

        this.callback = () =>
        {
            tmp();
            callback();
        };

        return this;
    }

    public SequenceEx Play() => Head.PlayThrough();
    private SequenceEx Head => prev == null ? this : prev.Head;

    private SequenceEx PlayThrough()
    {
        isPlaying = true;

        callback();

        if (list.Count == 0) PlayNext();

        foreach (var seqEx in list)
        {
            seqEx.Play();
        }

        return this;
    }

    private void PlayNext()
    {
        isPlaying = false;
        prev = null;
        next?.PlayThrough();
    }

    public SequenceEx Skip() => Head.Playing?.Complete();

    private SequenceEx Complete()
    {
        if (!isSkipable) return this;

        foreach (var seqEx in list)
        {
            seqEx.Complete(true);
        }
        return this;
    }
}
