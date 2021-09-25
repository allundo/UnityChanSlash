using UnityEngine;
using UniRx;
using DG.Tweening;
using System.Linq;

public class HandleIcon : FadeActivate
{
    [SerializeField] private FlickInteraction[] flicks = default;

    protected UITween ui;
    protected Tween prevTween = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        ui = new UITween(gameObject);
        Inactivate();
    }

    protected virtual void Start()
    {
        Observable
            .Merge(flicks.Select(flick => flick.Drag))
            .Subscribe(flick => SetFadeActive(flick))
            .AddTo(this);

        Observable
            .Merge(flicks.Select(flick => flick.FlickSubject))
            .Subscribe(_ => PlayTween(Apply()))
            .AddTo(this);

        Observable
            .Merge(flicks.Select(flick => flick.ReleaseSubject))
            .Subscribe(_ => PlayTween(FadeOut()))
            .AddTo(this);
    }

    public void Activate(Sprite sprite)
    {
        fade.SetSprite(sprite);
        base.Activate();
    }

    private void SetFadeActive(FlickInteraction.FlickDirection flick)
    {
        if (flick.Ratio > 0.5f)
        {
            if (!isActive) PlayTween(FadeIn(flick.sprite));
        }
        else
        {
            if (isActive) PlayTween(FadeOut());
        }
    }

    private Tween FadeIn(Sprite sprite, float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade.SetSprite(sprite))
            .AppendCallback(() => ui.ResetSize(2f))
            .Join(base.FadeIn(duration))
            .Join(ui.Resize(1.5f, duration));
    }

    private Tween FadeOut(float duration = 0.4f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration))
            .Join(ui.Resize(1f, duration));
    }

    private Tween Apply(float duration = 0.3f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration, null, null, false))
            .Join(ui.Resize(4f, duration));
    }

    private Tween PlayTween(Tween tween)
    {
        prevTween?.Kill();
        prevTween = tween.Play();
        return prevTween;
    }
}