using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using System.Linq;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] protected Image fadeImage = null;

    private Tween fadeIn = null;
    private Tween fadeOut = null;

    public virtual Vector2 sizeDelta
    {
        get { return fadeImage.rectTransform.sizeDelta; }
        set { fadeImage.rectTransform.sizeDelta = value; }
    }

    public virtual Color color
    {
        get { return fadeImage.color; }
        set { fadeImage.color = value; }
    }

    public virtual void SetAlpha(float alpha)
    {
        color = new Color(color.r, color.g, color.b, alpha);
    }

    protected virtual void Awake()
    {
        fadeImage = fadeImage ?? GetComponent<Image>();
        SetAlpha(0f);
    }

    public virtual void FadeIn(float duration = 1f, float delay = 0f, bool isContinuous = true, Ease ease = Ease.OutQuad)
    {
        // Fade out black image to display screen
        fadeIn = FadeFunc(false, duration, delay, isContinuous, ease);
    }

    public virtual void FadeOut(float duration = 1f, float delay = 0f, bool isContinuous = true, Ease ease = Ease.OutQuad)
    {
        // Fade in black image to hide screen
        fadeOut = FadeFunc(true, duration, delay, isContinuous, ease);
    }

    private Tween FadeFunc(bool isIn, float duration = 1f, float delay = 0f, bool isContinuous = true, Ease ease = Ease.OutQuad)
    {
        (isIn ? fadeIn : fadeOut)?.Kill();

        if (isContinuous)
        {
            duration *= isIn ? (1f - color.a) : color.a;
        }
        else
        {
            SetAlpha(isIn ? 0f : 1f);
        }

        return DOTween
            .ToAlpha(() => color, value => color = value, isIn ? 1f : 0f, duration)
            .SetUpdate(true)
            .SetEase(ease)
            .SetDelay(delay).Play();
    }

    public IObservable<Unit> FadeInObservable(float duration = 1f, float delay = 0f, Ease ease = Ease.OutQuad, params IObservable<Unit>[] asyncObservables)
        => FadeObservable(false, duration, delay, ease, asyncObservables);

    public IObservable<Unit> FadeOutObservable(float duration = 1f, float delay = 0f, Ease ease = Ease.OutQuad, params IObservable<Unit>[] asyncObservables)
        => FadeObservable(true, duration, delay, ease, asyncObservables);

    protected virtual IObservable<Unit> FadeObservable(bool isIn, float duration = 1f, float delay = 0f, Ease ease = Ease.OutQuad, params IObservable<Unit>[] asyncObservables)
    {
        var observableList = asyncObservables.ToList();
        observableList.Add(FadeFunc(isIn, duration, delay, true, ease).OnCompleteAsObservable<Unit>(Unit.Default));

        return Observable.Merge(observableList);
    }
}
