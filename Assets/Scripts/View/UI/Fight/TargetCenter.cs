using DG.Tweening;

public class TargetCenter : FadeEnable
{
    private UITween uiTween;
    private Tween blinkLoop;
    protected override void Awake()
    {
        fade = new FadeMaterialColor(gameObject, 0.2f);
        uiTween = new UITween(gameObject);

        fade.SetAlpha(0f);

        blinkLoop = fade.ToAlpha(0.25f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo).AsReusable(gameObject);
    }
    public void FadeActivate()
    {
        FadeIn(0.2f, null, () => blinkLoop.Restart()).Play();
    }

    public void FadeInactivate()
    {
        blinkLoop.Rewind();
        FadeOut(0.1f).Play();
    }
}