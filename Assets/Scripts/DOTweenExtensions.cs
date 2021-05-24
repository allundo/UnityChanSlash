using UnityEngine;
using System;
using DG.Tweening;
using UniRx;

static public class DOTweenExtensions
{
    static public Tween AsReusable(this Tween tween, GameObject linktarget)
    {
        return tween.Pause().SetAutoKill(false).SetLink(linktarget);
    }

    static public IObservable<Tween> OnCompleteAsObservable(this Tween tween)
    {
        return Observable.Create<Tween>(o =>
        {
            TweenCallback onComplete = tween.onComplete;

            tween.OnComplete(() =>
            {
                onComplete();
                o.OnNext(tween);
                o.OnCompleted();
            });
            return Disposable.Create(() =>
            {
                tween.Kill();
            });
        });
    }

    static public IObservable<Tween> AsObservable(this Tween tween)
    {
        return Observable.Create<Tween>(o =>
        {
            TweenCallback onPlay = tween.onPlay;

            tween.OnPlay(() =>
            {
                onPlay();
                o.OnNext(tween);
            });

            TweenCallback onComplete = tween.onComplete;

            tween.OnComplete(() =>
            {
                onComplete();
                o.OnCompleted();
            });

            return Disposable.Create(() =>
            {
                tween.Kill();
            });
        });
    }
}