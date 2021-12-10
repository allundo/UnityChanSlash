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

    protected override void Awake()
    {
        base.Awake();
        forward = fade.sprite;

        dashTimer = DOTween.Sequence()
            .AppendCallback(() => fade.sprite = dash)
            .AppendInterval(duration)
            .AppendCallback(() => fade.sprite = forward)
            .AsReusable(gameObject);

        walkTimer = DOTween.Sequence()
            .AppendCallback(() => isWalking = false)
            .AppendInterval(duration)
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

        dashTimer.Pause();
        isDash.Value = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif

        if (!isActive || isWalking) return;
        dashTimer.Restart();
    }

    public override void ReleaseButton()
    {
        if (!isPressed.Value) return;

        isPressed.Value = false;
        isDash.Value = false;

        shrink = ui.Resize(1f, 0.2f).Play();
        alphaTween = fade.ToAlpha(maxAlpha, 0.3f).Play();
        fade.sprite = forward;
    }

    protected override void OnFadeIn()
    {
        fade.sprite = forward;
        ui.ResetSize();
    }

    protected override void OnFadeOut()
    {
        isDash.Value = false;
        isPressed.Value = false;
    }
}
