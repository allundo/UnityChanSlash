using DG.Tweening;
using UnityEngine;

public class TargetCorner : FadeEnable
{
    [SerializeField] private Color pointerOnColor = default;

    private UITween uiTween;
    private Tween activate;
    private Tween expansionLoop;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.3f);
        uiTween = new UITween(gameObject);

        fade.SetAlpha(0f);

        activate = uiTween.Resize(1f, 0.2f)
            .OnComplete(ExpansionLoop)
            .SetUpdate(false)
            .AsReusable(gameObject);
    }

    protected void ExpansionLoop()
    {
        expansionLoop?.Kill();

        uiTween.ResetSize();

        expansionLoop = DOTween.Sequence()
            .Join(uiTween.Resize(1.1f, 0.5f).SetEase(Ease.InQuad))
            .Join(fade.ToAlpha(0.4f, 0.5f).SetEase(Ease.InQuad))
            .SetUpdate(false)
            .SetLoops(-1, LoopType.Yoyo)
            .Play();
    }

    public void FadeActivate()
    {
        activate.Rewind();

        FadeIn(0.2f).Play();
        uiTween.ResetSize(2f);
        activate.Restart();
    }

    public void FadeInactivate()
    {
        expansionLoop?.Kill();
        FadeOut(0.1f, null, () => activate.Rewind()).Play();
    }

    public void SetPointerOn()
    {
        expansionLoop.Pause();
        fade.KillTweens();
        uiTween.ResetSize(1.1f);
        fade.color = pointerOnColor;
    }

    public void SetPointerOff()
    {
        expansionLoop.Restart();
        fade.ResetColor();
    }
}
