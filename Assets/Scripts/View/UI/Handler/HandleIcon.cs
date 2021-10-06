using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using System.Linq;

[RequireComponent(typeof(Image))]
public class HandleIcon : FadeEnable
{
    [SerializeField] private FlickInteraction[] flicks = default;
    [SerializeField] private HandleText text = default;

    protected UITween ui;
    protected Tween prevTween = null;
    private FlickInteraction.FlickDirection currentFlick = null;

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
            .Merge(flicks.Select(flick => flick.IsReleased))
            .Subscribe(_ => Disable())
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
            if (!isActive)
            {
                PlayTween(Show(flick));
            }
            else if (flick != currentFlick)
            {
                PlayTween(Switch(flick));
            }

            currentFlick = flick;
        }
        else if (isActive)
        {
            Disable();
            currentFlick = null;
        }
    }

    private Tween Show(FlickInteraction.FlickDirection flick, float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade.SetSprite(flick.sprite))
            .AppendCallback(() => ui.ResetSize(2f))
            .Join(base.FadeIn(duration))
            .Join(ui.Resize(1.5f, duration))
            .Join(text.Show(flick.text));
    }
    private Tween Switch(FlickInteraction.FlickDirection flick, float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade.SetSprite(flick.sprite))
            .AppendCallback(() => ui.ResetSize(2f))
            .Join(base.FadeIn(duration, null, null, false))
            .Join(ui.Resize(1.5f, duration))
            .Join(text.Switch(flick.text));
    }

    private Tween Hide(float duration = 0.4f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration))
            .Join(ui.Resize(1f, duration))
            .Join(text.Hide());
    }

    public Tween Disable() => PlayTween(Hide());

    private Tween Apply(float duration = 0.3f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration, null, null, false))
            .Join(ui.Resize(4f, duration))
            .Join(text.Apply());
    }

    private Tween PlayTween(Tween tween)
    {
        prevTween?.Kill();
        prevTween = tween.Play();
        return prevTween;
    }
}