using DG.Tweening;
using UnityEngine;

public class TargetCorner : FadeUI, ITargetUI
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

        activate?.Complete();
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
        activate?.Complete();
        expansionLoop?.Kill();
        uiTween.ResetSize(2f);
        activate = uiTween.Resize(1f, fadeDuration).Play();
    }
    protected override void OnCompleteFadeIn() => ExpansionLoop();

    protected override void BeforeFadeOut() => expansionLoop?.Kill();
    protected override void OnDisable() => activate?.Complete();

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
