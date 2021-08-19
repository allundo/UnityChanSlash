﻿using UnityEngine;
using DG.Tweening;

public class MessageWindowUI : FadeActivate
{
    private UITween uiTween;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f, true);
        uiTween = new UITween(gameObject, true);

        uiTween.SetSize(new Vector2(uiTween.defaultSize.x, 0f));

    }

    public override Tween Activate(float duration = 1f, TweenCallback onComplete = null)
    {
        uiTween.ResizeY(1f, 0.5f).Play();
        return base.Activate(duration, onComplete);
    }

    public override Tween Inactivate(float duration = 1f, TweenCallback onComplete = null)
    {
        uiTween.ResizeY(0f, 0.5f).Play();
        return base.Inactivate(duration, onComplete);
    }
}
