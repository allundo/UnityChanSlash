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

    protected virtual Color color
    {
        get
        {
            return image.color;
        }
        set
        {
            image.color = value;
        }
    }

    protected float AlphaRatio => color.a / maxAlpha;

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
        color = new Color(color.r, color.g, color.b, alpha * maxAlpha);
    }

    public virtual Tween In(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        fadeIn = Fade(maxAlpha, duration * (1f - AlphaRatio), delay)
            .OnPlay(() =>
            {
                fadeOut?.Kill();
                onPlay();
            })
            .OnComplete(onComplete);

        return fadeIn;
    }

    public virtual Tween Out(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        fadeOut = Fade(0, duration * AlphaRatio, delay)
            .OnPlay(() =>
            {
                fadeIn?.Kill();
                onPlay();
            })
            .OnComplete(onComplete);

        return fadeOut;
    }

    private Tween Fade(float alpha, float duration = 1f, float delay = 0f)
    {
        return
            DOTween
                .ToAlpha(() => color, value => color = value, alpha, duration)
                .SetUpdate(isValidOnPause)
                .SetDelay(delay);
    }
}
