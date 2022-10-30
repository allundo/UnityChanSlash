using DG.Tweening;
using UnityEngine;

public class TargetCenter : FadeUI, ITargetUI
{
    [SerializeField] private Color pointerOnColor = default;

    private Tween blinkLoop;
    private RectTransform rectTransform;
    private Vector2 defaultSize;

    protected override void Awake()
    {
        FadeInit(new FadeMaterialColor(gameObject, maxAlpha));
        blinkLoop = fade.ToAlpha(0.25f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo).AsReusable(gameObject);
        rectTransform = GetComponent<RectTransform>();
        defaultSize = rectTransform.sizeDelta;
    }

    public void SetPointerOn()
    {
        blinkLoop.Pause();
        fade.KillTweens();
        rectTransform.sizeDelta = defaultSize * 1.1f;
        fade.color = pointerOnColor;
    }

    public void SetPointerOff()
    {
        rectTransform.sizeDelta = defaultSize;
        blinkLoop.Restart();
        fade.ResetColor();
    }
}