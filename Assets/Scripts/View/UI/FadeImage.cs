using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeImage : MonoBehaviour
{
    [SerializeField] protected float maxAlpha = 1.0f;

    protected Image image;
    private Tween fadeIn = null;
    private Tween fadeOut = null;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
    }

    protected void SetAlpha(float alpha)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha * maxAlpha);
    }

    public virtual Tween FadeIn(float duration = 1f, float delay = 0f)
    {
        fadeIn = FadeTween(maxAlpha, duration, delay).OnPlay(() => fadeOut?.Kill());
        return fadeIn;
    }

    public virtual Tween FadeOut(float duration = 1f, float delay = 0f)
    {
        fadeOut = FadeTween(0, duration, delay).OnPlay(() => fadeIn?.Kill());
        return fadeOut;
    }

    public virtual Tween OnPauseFadeIn(float duration = 1f, float delay = 0f)
        => FadeIn(duration, delay).SetUpdate(true);

    public virtual Tween OnPauseFadeOut(float duration = 1f, float delay = 0f)
        => FadeOut(duration, delay).SetUpdate(true);

    private Tween FadeTween(float alpha, float duration = 1f, float delay = 0f)
    {
        return
            DOTween
                .ToAlpha(() => image.color, color => image.color = color, alpha, duration)
                .SetDelay(delay);
    }
}
