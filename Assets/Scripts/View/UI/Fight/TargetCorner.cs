using DG.Tweening;

public class TargetCorner : FadeEnable
{
    private UITween uiTween;
    private Tween activate;
    private Tween expansionLoop;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.3f);
        uiTween = new UITween(gameObject);

        fade.SetAlpha(0f);

        activate = uiTween.Resize(1f, 0.2f)
            .AsReusable(gameObject)
            .OnComplete(() => expansionLoop.Restart());

        expansionLoop = DOTween.Sequence()
            .Join(uiTween.Resize(1.1f, 0.5f).SetEase(Ease.InQuad))
            .Join(fade.ToAlpha(0.4f, 0.5f).SetEase(Ease.InQuad))
            .SetUpdate(false)
            .SetLoops(-1, LoopType.Yoyo)
            .AsReusable(gameObject);
    }

    public void FadeActivate()
    {
        FadeIn(0.2f).Play();
        uiTween.ResetSize(2f);
        activate.Restart();
    }

    public void FadeInactivate()
    {
        activate.Rewind();
        expansionLoop.Rewind();
        FadeOut(0.1f).Play();
    }
}