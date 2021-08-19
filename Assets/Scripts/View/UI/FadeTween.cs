using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeTween
{
    protected MaskableGraphic image;
    protected float maxAlpha;
    protected bool isValidOnPause;

    private Tween fadeIn = null;
    private Tween fadeOut = null;

    protected float AlphaRatio => image.color.a / maxAlpha;

    public FadeTween(MaskableGraphic image, float maxAlpha = 1f, bool isValidOnPause = false)
    {
        this.image = image;
        this.maxAlpha = maxAlpha;
        this.isValidOnPause = isValidOnPause;
    }
    public FadeTween(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : this(gameObject.GetComponent<MaskableGraphic>(), maxAlpha, isValidOnPause) { }

    public void SetAlpha(float alpha)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha * maxAlpha);
    }

    public virtual Tween In(float duration = 1f, float delay = 0f)
    {
        fadeIn = Fade(maxAlpha, duration * (1f - AlphaRatio), delay).SetUpdate(isValidOnPause).OnPlay(() => fadeOut?.Kill());
        return fadeIn;
    }

    public virtual Tween Out(float duration = 1f, float delay = 0f)
    {
        fadeOut = Fade(0, duration * AlphaRatio, delay).SetUpdate(isValidOnPause).OnPlay(() => fadeIn?.Kill());
        return fadeOut;
    }

    private Tween Fade(float alpha, float duration = 1f, float delay = 0f)
    {
        return
            DOTween
                .ToAlpha(() => image.color, color => image.color = color, alpha, duration)
                .SetDelay(delay);
    }
}
