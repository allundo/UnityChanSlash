using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using System.Linq;

public class CoverScreen : FadeScreen
{
    [SerializeField] protected Image coverImage = null;

    protected FadeMaterialColor fadeMC = null;

    public override Vector2 sizeDelta
    {
        get
        {
            return coverImage.rectTransform.sizeDelta;
        }
        set
        {
            if (fadeImage != null) fadeImage.rectTransform.sizeDelta = value;
            coverImage.rectTransform.sizeDelta = value;
        }
    }

    public override void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            base.SetAlpha(alpha);
            fadeMC.SetAlpha(0f);
        }
        else
        {
            fadeMC.SetAlpha(alpha);
        }
    }

    protected override void Awake()
    {
        coverImage = coverImage ?? GetComponent<Image>();
        fadeMC = new FadeMaterialColor(coverImage, 1f, true);

        if (fadeImage != null) base.SetAlpha(0f);

        fadeMC.SetAlpha(0f);
    }

    public void CoverOff(float duration = 1f, float delay = 0f)
    {
        if (fadeImage != null) base.SetAlpha(0f);

        // Fade out black image to display screen
        fadeMC.Out(duration, delay).SetEase(Ease.InQuad).SetUpdate(true).Play();
    }

    public void CoverOn(float duration = 1f, float delay = 0f)
    {
        if (fadeImage != null) base.SetAlpha(0f);

        // Fade in black image to display screen
        fadeMC.In(duration, delay).SetEase(Ease.OutQuad).SetUpdate(true).Play();
    }

    public IObservable<Unit> CoverOnObservable(float duration = 1f, params IObservable<Unit>[] asyncObservables)
    {
        if (fadeImage != null) base.SetAlpha(0f);

        var observableList = asyncObservables.ToList();
        observableList.Add(fadeMC.In(duration).SetEase(Ease.OutQuad).SetUpdate(true).OnCompleteAsObservable<Unit>(Unit.Default));

        return Observable.Merge(observableList);
    }

    public override void FadeIn(float duration = 1f, float delay = 0f, bool isContinuous = true, Ease ease = Ease.OutQuad)
    {
        fadeMC.SetAlpha(0f);
        base.FadeIn(duration, delay, isContinuous, ease);
    }

    public override void FadeOut(float duration = 1f, float delay = 0f, bool isContinuous = true, Ease ease = Ease.OutQuad)
    {
        fadeMC.SetAlpha(0f);
        base.FadeOut(duration, delay, isContinuous, ease);
    }

    protected override IObservable<Unit> FadeObservable(bool isIn, float duration = 1f, float delay = 0f, Ease ease = Ease.OutQuad, params IObservable<Unit>[] asyncObservables)
    {
        fadeMC.SetAlpha(0f);
        return base.FadeObservable(isIn, duration, delay, ease, asyncObservables);
    }

    private void OnDestroy()
    {
        fadeMC.OnDestroy();
    }
}
