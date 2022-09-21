using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class RestartUI : FadeEnable
{
    [SerializeField] private FadeEnable descriptionTxt = default;
    [SerializeField] private RestartWindowUI selectUI = default;

    public IObservable<Tween> Restart => selectUI.restartButton.OnPressedCompleteAsObservable();
    public IObservable<Tween> Title => selectUI.titleButton.OnPressedCompleteAsObservable();

    private Sequence sequence = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
        Inactivate();
    }

    protected virtual void Start()
    {
        sequence = DOTween.Sequence()
            .Join(FadeIn(1f))
            .Join(selectUI.FadeIn(0.5f).OnComplete(() => descriptionTxt.FadeIn(0.5f).Play()))
            .AppendCallback(selectUI.ActivateButtons);

        Restart.Subscribe(_ => GetCloseTween(0.5f).Play()).AddTo(this);
        Title.Subscribe(_ => GetCloseTween(1.5f).Play()).AddTo(this);
    }

    protected Tween GetCloseTween(float fadeDuration = 0.5f)
    {
        return DOTween.Sequence()
            .AppendCallback(selectUI.InactivateButtons)
            .AppendCallback(() => FadeOut(fadeDuration).Play())
            .AppendCallback(() => selectUI.FadeOut(1f).Play())
            .Append(descriptionTxt.FadeOut(0.2f))
            .AppendInterval(0.3f);
    }

    public IObservable<Unit> Play()
    {
        gameObject.SetActive(true);
        return sequence.OnCompleteAsObservable(Unit.Default);
    }
}
