using UnityEngine;
using UniRx;
using DG.Tweening;

public class ForwardButton : MoveButton
{
    [SerializeField] protected Sprite dash = default;
    [SerializeField] protected float duration = 0.2f;

    private Sprite forward;
    private Tween dashTimer;
    private Tween walkTimer;

    protected IReactiveProperty<bool> isDash = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsDash => isDash;

    private bool isWalking = false;
    private bool isDashValid = false;
    public bool isDashInputActive = false;

    protected override void Awake()
    {
        base.Awake();
        forward = fade.sprite;

        dashTimer = DOTween.Sequence()
            .AppendInterval(duration)
            .AppendCallback(DisableDash)
            .AsReusable(gameObject);

        walkTimer = DOTween.Sequence()
            .AppendCallback(() => isWalking = false)
            .AppendInterval(duration * 2f)
            .AppendCallback(() => isWalking = true)
            .AsReusable(gameObject);

    }

    public void PointerDown()
    {
        if (!isActive) return;

        if (isDashValid)
        {
            dashTimer.Pause();
            isDash.Value = true;
            isDashValid = false;
        }
    }

    public void PointerUp()
    {
        if (!isActive || !isDashInputActive || isWalking || !isPressed.Value) return;
        ActivateDashTimer();
    }

    /// <summary>
    /// Called by PointerEnter()
    /// </summary>
    public override void PressButton()
    {
        if (isPressed.Value) return;

        isPressed.Value = true;
        shrink?.Kill();
        ui.ResetSize(1.5f);
        alphaTween?.Kill();
        alphaTween = fade.ToAlpha(uiAlpha, 0.1f).Play();

        walkTimer.Restart();
    }

    /// <summary>
    /// Called by PointerExit()
    /// </summary>
    public override void ReleaseButton()
    {
        if (!isPressed.Value) return;
        isPressed.Value = false;

        if (isDash.Value)
        {
            isDash.Value = false;
            DisableDash();
        }

        shrink = ui.Resize(1f, 0.2f).Play();
        alphaTween = fade.ToAlpha(uiAlpha * maxAlpha, 0.3f).Play();
    }

    protected override void OnFadeIn()
    {
        DisableDash();
        ui.ResetSize();
    }

    protected override void OnFadeOut()
    {
        isDash.Value = false;
        isPressed.Value = false;
    }

    private void ActivateDashTimer()
    {
        fade.sprite = dash;
        isDashValid = true;
        dashTimer.Restart();
    }

    protected void DisableDash()
    {
        fade.sprite = forward;
        isDashValid = false;
    }
}
