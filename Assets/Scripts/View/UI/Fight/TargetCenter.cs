using DG.Tweening;
using UnityEngine;

public class TargetCenter : FadeEnable
{
    [SerializeField] private Color pointerOnColor = default;

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

    public void SetPointerOn()
    {
        blinkLoop.Pause();
        fade.KillTweens();
        uiTween.ResetSize(1.1f);
        fade.color = pointerOnColor;
    }

    public void SetPointerOff()
    {
        blinkLoop.Restart();
        fade.ResetColor();
    }
}