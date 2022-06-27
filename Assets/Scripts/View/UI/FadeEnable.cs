﻿using UnityEngine;
using DG.Tweening;

public class FadeEnable : MonoBehaviour
{
    public bool isActive { get; protected set; } = true;
    protected FadeTween fade;

    protected virtual void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        Inactivate();
    }

    protected virtual void Activator() => fade.Enable();
    protected virtual void Inactivator() => fade.Disable();

    public virtual void Activate()
    {
        Activator();
        isActive = true;
        fade.KillTweens();
        fade.SetAlpha(1f);
    }

    public virtual void Inactivate()
    {
        isActive = false;
        fade.KillTweens();
        fade.SetAlpha(0f);
        Inactivator();
    }

    protected virtual void OnFadeIn() { }
    protected virtual void OnFadeOut() { }

    public virtual Tween FadeIn(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return fade.In(duration, 0f,
            () =>
            {
                Activator();
                isActive = true;
                OnFadeIn();
                onPlay();
            },
            onComplete,
            isContinuous);
    }

    public virtual Tween FadeOut(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return fade.Out(duration, 0f,
            () =>
            {
                isActive = false;
                OnFadeOut();
                onPlay();
            },
            () =>
            {
                onComplete();
                Inactivator();
            },
            isContinuous);
    }

    private void OnDestroy()
    {
        fade?.OnDestroy();
    }
}
