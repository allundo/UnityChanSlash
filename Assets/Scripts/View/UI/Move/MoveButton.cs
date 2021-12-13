﻿using UnityEngine;
using DG.Tweening;
using UniRx;

public class MoveButton : FadeEnable
{
    [SerializeField] protected float maxAlpha = DEFAULT_ALPHA;
    [SerializeField] protected Vector2 defaultSize = new Vector2(100f, 100f);
    [SerializeField] protected Vector2 fightingOffset = default;

    private static readonly float DEFAULT_ALPHA = 0.4f;

    protected UITween ui;

    protected IReactiveProperty<bool> isPressed = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsPressed => isPressed;

    protected Tween shrink = null;
    protected Tween alphaTween = null;
    protected Tween fightExpand = null;
    protected Tween moveDefault = null;

    protected bool isFighting = false;


    protected override void Awake()
    {
        fade = new FadeTween(gameObject, maxAlpha, true);
        ui = new UITween(gameObject);

        // BUG: Initialize on awake in case sizeDelta is set to zero at scene transition
        ui.SetSize(defaultSize, true);

        ui.ResetSize();
        fade.SetAlpha(maxAlpha);

        Inactivate();
    }

    protected virtual void Start()
    {
        fightExpand = ui.MoveOffset(fightingOffset, 0.05f, true);
        moveDefault = ui.MoveBack(0.2f, true);
    }

    protected override void OnFadeIn() => ui.ResetSize();
    protected override void OnFadeOut() => isPressed.Value = false;

    public virtual void PressButton()
    {
        if (isPressed.Value) return;

        isPressed.Value = true;
        shrink?.Kill();
        ui.ResetSize(1.5f);
        alphaTween?.Kill();
        alphaTween = fade.ToAlpha(1.0f, 0.1f).Play();
    }

    public virtual void ReleaseButton()
    {
        if (!isPressed.Value) return;

        isPressed.Value = false;
        shrink = ui.Resize(1f, 0.2f).Play();
        alphaTween = fade.ToAlpha(maxAlpha, 0.3f).Play();
    }

    public override void Activate()
    {
        base.Activate();
        OnFadeIn();
    }

    public override void Inactivate()
    {
        OnFadeOut();
        shrink?.Kill();
        alphaTween?.Kill();
        base.Inactivate();
    }

    public void SetFightingPos(bool isFighting)
    {
        if (isFighting)
        {
            MoveFight();
        }
        else
        {
            MoveDefault();
        }
    }

    private void MoveFight()
    {
        if (isFighting) return;

        isFighting = true;
        moveDefault?.Pause();
        fightExpand?.Restart();
    }

    private void MoveDefault()
    {
        if (!isFighting) return;

        isFighting = false;
        fightExpand?.Pause();
        moveDefault?.Restart();
    }
}
