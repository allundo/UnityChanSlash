using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeTween
{
    protected MaskableGraphic image;
    protected float maxAlpha;
    protected Color defaultColor;
    protected bool isValidOnPause;

    private Tween fadeIn = null;
    private Tween fadeOut = null;

    public virtual Color color
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

    public Sprite sprite
    {
        get
        {
            CheckImage();
            return (image as Image).sprite;
        }
        set
        {
            CheckImage();
            (image as Image).sprite = value;
        }
    }

    private void CheckImage()
    {
        if (!(image is Image)) throw new TypeAccessException("Failed to set Sprite. Target is not an Image.");
    }

    protected float AlphaRatio => color.a / maxAlpha;

    public FadeTween(MaskableGraphic image, float maxAlpha = 1f, bool isValidOnPause = false)
    {
        this.image = image;
        this.maxAlpha = maxAlpha;
        this.isValidOnPause = isValidOnPause;
        if (image != null) defaultColor = image.color;
    }
    public FadeTween(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : this(gameObject.GetComponent<MaskableGraphic>(), maxAlpha, isValidOnPause) { }

    public void KillTweens()
    {
        fadeIn?.Kill();
        fadeOut?.Kill();
    }

    public void CompleteTweens()
    {
        fadeIn?.Complete(true);
        fadeOut?.Complete(true);
    }

    public void SetAlpha(float alpha, bool isScaledByMaxAlpha = true)
    {
        color = new Color(color.r, color.g, color.b, isScaledByMaxAlpha ? alpha * maxAlpha : alpha);
    }

    public void ResetColor(bool setAlphaDefault = false)
    {
        color = setAlphaDefault ? defaultColor : new Color(defaultColor.r, defaultColor.g, defaultColor.b, color.a);
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
        fadeIn = FadeFunc(true, duration, delay, onPlay, onComplete, isContinuous);
        return fadeIn;
    }

    public virtual Tween Out(float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        fadeOut = FadeFunc(false, duration, delay, onPlay, onComplete, isContinuous);
        return fadeOut;
    }

    private Tween FadeFunc(bool isIn, float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        if (isContinuous) return FadeFunc(isIn, duration * (isIn ? (1f - AlphaRatio) : AlphaRatio), delay, onPlay, onComplete);

        return DOTween.Sequence()
            .AppendCallback(() => SetAlpha(isIn ? 0f : 1f))
            .Append(FadeFunc(isIn, duration, delay, onPlay, onComplete))
            .SetUpdate(isValidOnPause);
    }

    private Tween FadeFunc(bool isIn, float duration = 1f, float delay = 0f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return ToAlpha(isIn ? maxAlpha : 0f, duration, delay)
            .OnPlay(() =>
            {
                (isIn ? fadeOut : fadeIn)?.Kill();
                onPlay();
            })
            .OnComplete(onComplete);
    }

    public Tween ToAlpha(float alpha, float duration = 1f, float delay = 0f)
    {
        return DOTween
            .ToAlpha(() => color, value => color = value, alpha, duration)
            .SetUpdate(isValidOnPause)
            .SetDelay(delay);
    }

    public virtual void OnDestroy() { }

    public virtual Tween DOColor(Color endValue, float duration) => image.DOColor(endValue, duration);
}
