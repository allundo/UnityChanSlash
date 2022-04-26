using DG.Tweening;
using System;

public class TweenUtil
{
    public Tween prevTween { get; protected set; } = null;

    public Tween PlayExclusive(Tween newTween, bool complete = true)
        => PlayExclusive(newTween, complete ? CompleteTween : KillTween);

    public Tween PlayExclusive(Tween newTween, Action<Tween> finish)
    {
        if (newTween == null) return null;

        finish(prevTween);
        prevTween = newTween;

        return newTween.Play();
    }

    public Action<Tween> KillTween = tween => tween?.Kill();
    public void Kill() => KillTween(prevTween);
    public Action<Tween> CompleteTween = tween => tween?.Complete(true);
    public void Complete() => CompleteTween(prevTween);
}
