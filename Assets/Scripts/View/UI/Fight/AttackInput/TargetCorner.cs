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
        fade = new FadeTween(gameObject, 0.4f);
        uiTween = new UITween(gameObject);

        fade.SetAlpha(0f);
    }

    protected void ExpansionLoop()
    {
        if (expansionLoop != null && expansionLoop.IsActive()) return;

        uiTween.ResetSize();
        fade.SetAlpha(1f);

        expansionLoop = DOTween.Sequence()
            .Join(uiTween.Resize(1.1f, 0.5f).SetEase(Ease.InQuad))
            .Join(fade.ToAlpha(0.25f, 0.5f).SetEase(Ease.InQuad))
            .SetUpdate(false)
            .SetLoops(-1, LoopType.Yoyo)
            .Play();
    }

    public void FadeActivate()
    {
        activate?.Kill();
        uiTween.ResetSize(2f);

        FadeIn(0.2f).Play();

        activate = uiTween.Resize(1f, 0.2f)
            .OnComplete(ExpansionLoop)
            .SetUpdate(false)
            .Play();
    }

    public void FadeInactivate()
    {
        expansionLoop?.Kill();
        FadeOut(0.1f, null, () => activate?.Kill()).Play();
    }

    public void SetPointerOn()
    {
        expansionLoop?.Kill();
        fade.KillTweens();
        uiTween.ResetSize(1.1f);
        fade.color = pointerOnColor;
    }

    public void SetPointerOff()
    {
        ExpansionLoop();
        fade.ResetColor();
    }
}
