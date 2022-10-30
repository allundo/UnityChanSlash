using DG.Tweening;
using UnityEngine;

public class TargetCorner : FadeUI
{
    [SerializeField] private Color pointerOnColor = default;

    private UITween uiTween;
    private Tween activate;
    private Tween expansionLoop;

    protected override void Awake()
    {
        base.Awake();
        uiTween = new UITween(gameObject);
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

    protected override void OnFadeEnable(float fadeDuration)
    {
        activate?.Kill();
        uiTween.ResetSize(2f);
        activate = uiTween.Resize(1f, fadeDuration).Play();
    }
    protected override void OnCompleteFadeIn() => ExpansionLoop();

    protected override void BeforeFadeOut() => expansionLoop?.Kill();
    protected override void OnDisable() => activate?.Kill();

    public override void SetPointerOn()
    {
        expansionLoop?.Kill();
        fade.KillTweens();
        uiTween.ResetSize(1.1f);
        fade.color = pointerOnColor;
    }

    public override void SetPointerOff()
    {
        ExpansionLoop();
        fade.ResetColor();
    }
}
