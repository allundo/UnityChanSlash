using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeScreen : MonoBehaviour
{
    private Image black;
    private Tween fadeIn = null;
    private Tween fadeOut = null;

    void Awake()
    {
        black = GetComponent<Image>();
    }

    public Tween FadeIn(float duration = 1f)
    {
        fadeIn = DOTween
            .ToAlpha(() => black.color, color => black.color = color, 0f, duration)
            .OnPlay(() => fadeOut?.Kill());

        return fadeIn;
    }

    public Tween FadeOut(float duration = 1f)
    {
        fadeOut = DOTween
            .ToAlpha(() => black.color, color => black.color = color, 1f, duration)
            .OnPlay(() => fadeIn?.Kill());

        return fadeOut;
    }
}
