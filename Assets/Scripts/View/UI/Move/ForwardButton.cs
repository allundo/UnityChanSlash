using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class ForwardButton : PointerEnter, IPointerDownHandler, IPointerUpHandler
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
            .AppendCallback(EnableDash)
            .AppendInterval(duration)
            .AppendCallback(DisableDash)
            .AsReusable(gameObject);

        walkTimer = DOTween.Sequence()
            .AppendCallback(() => isWalking = false)
            .AppendInterval(duration * 2f)
            .AppendCallback(() => isWalking = true)
            .AsReusable(gameObject);

    }

#if UNITY_EDITOR
    // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
    private InputControl ic = new InputControl();
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif
        if (!isActive) return;

        if (isDashValid)
        {
            dashTimer.Pause();
            isDash.Value = true;
            isDashValid = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif

        if (!isActive || !isDashInputActive || isWalking) return;
        dashTimer.Restart();
    }

    public override void PressButton()
    {
        if (isPressed.Value) return;

        isPressed.Value = true;
        shrink?.Kill();
        ui.ResetSize(1.5f);
        alphaTween?.Kill();
        alphaTween = fade.ToAlpha(1.0f, 0.1f).Play();

        walkTimer.Restart();
    }

    public override void ReleaseButton()
    {
        isDash.Value = false;
        DisableDash();

        if (!isPressed.Value) return;

        isPressed.Value = false;

        shrink = ui.Resize(1f, 0.2f).Play();
        alphaTween = fade.ToAlpha(maxAlpha, 0.3f).Play();
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

    protected void EnableDash()
    {
        fade.sprite = dash;
        isDashValid = true;
    }

    protected void DisableDash()
    {
        fade.sprite = forward;
        isDashValid = false;
    }
}
