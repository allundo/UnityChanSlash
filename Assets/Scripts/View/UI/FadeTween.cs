using System;
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

    public void KillTweens()
    {
        fadeIn?.Kill();
        fadeOut?.Kill();
    }

    public void SetAlpha(float alpha, bool isScaledByMaxAlpha = true)
    {
        color = new Color(color.r, color.g, color.b, isScaledByMaxAlpha ? alpha * maxAlpha : alpha);
    }

    public void SetSprite(Sprite sprite)
    {
        if (!(image is Image)) throw new TypeAccessException("Failed to set Sprite. Target is not an Image.");

        (image as Image).sprite = sprite;
    }

    public void SetEnabled(bool isEnable = true)
    {
        image.enabled = isEnable;
    }

    public void Enable() => SetEnabled(true);
    public void Disable() => SetEnabled(false);

    public virtual void SetActive(bool isActive = true)
    {
        image.gameObject.SetActive(isActive);
    }

    public void Activate() => SetActive(true);
    public void Inactivate() => SetActive(false);

    public virtual Tween In(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        if (isContinuous) return In(duration * (1f - AlphaRatio), delay, onPlay, onComplete);

        return DOTween.Sequence()
            .AppendCallback(() => SetAlpha(0f))
            .Append(In(duration, delay, onPlay, onComplete));
    }

    private Tween In(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        fadeIn = ToAlpha(maxAlpha, duration, delay)
            .OnPlay(() =>
            {
                fadeOut?.Kill();
                onPlay();
            })
            .OnComplete(onComplete);

        return fadeIn;
    }

    public virtual Tween Out(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        if (isContinuous) return Out(duration * AlphaRatio, delay, onPlay, onComplete);

        return DOTween.Sequence()
            .AppendCallback(() => SetAlpha(1f))
            .Append(Out(duration, delay, onPlay, onComplete));
    }

    private Tween Out(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        fadeOut = ToAlpha(0, duration, delay)
            .OnPlay(() =>
            {
                fadeIn?.Kill();
                onPlay();
            })
            .OnComplete(onComplete);

        return fadeOut;
    }

    public Tween ToAlpha(float alpha, float duration = 1f, float delay = 0f)
    {
        return
            DOTween
                .ToAlpha(() => color, value => color = value, alpha, duration)
                .SetUpdate(isValidOnPause)
                .SetDelay(delay);
    }
}
