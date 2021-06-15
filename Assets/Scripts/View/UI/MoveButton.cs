using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class MoveButton : MonoBehaviour
{
    [SerializeField] float maxAlpha = 1.0f;

    protected RectTransform rectTransform;
    protected Image image;
    private Vector2 defaultSize;

    protected IReactiveProperty<bool> isPressed = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsPressed => isPressed;

    protected Tween shrink = null;
    protected Tween defaultAlpha = null;

    public Vector2 Position => rectTransform.anchoredPosition;
    public Vector2 Size => rectTransform.sizeDelta;
    public float Radius => Size.x * 0.5f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Start()
    {
        defaultSize = Size;

        ResetSize();
        SetAlpha(0.4f);
    }

    protected Tween GetResize(float ratio = 1.5f, float duration = 0.2f, bool isReusable = false)
    {
        Tween resize = rectTransform.DOSizeDelta(defaultSize * ratio, duration);
        return isReusable ? resize.AsReusable(gameObject) : resize;
    }

    protected Tween GetToAlpha(float alpha, float duration = 0.2f, bool isReusable = false)
    {
        Tween toAlpha = DOTween.ToAlpha(() => image.color, c => image.color = c, alpha * maxAlpha, duration);
        return isReusable ? toAlpha.AsReusable(gameObject) : toAlpha;
    }

    protected void ResetSize()
    {
        Resize(1.0f);
    }

    protected void Resize(float ratio)
    {
        rectTransform.sizeDelta = defaultSize * ratio;
    }

    public virtual void PressButton()
    {
        if (isPressed.Value) return;

        isPressed.Value = true;
        shrink?.Kill();
        defaultAlpha?.Kill();
        Resize(1.5f);
        SetAlpha(1.0f);
    }

    public virtual void ReleaseButton()
    {
        if (!isPressed.Value) return;

        isPressed.Value = false;
        shrink = GetResize(1.0f).Play();
        defaultAlpha = GetToAlpha(0.4f).Play();
    }

    public void SetAlpha(float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }

    public void Activate(float alpha)
    {
        SetAlpha(alpha);
        ResetSize();
        gameObject.SetActive(true);
    }

    public void Inactivate()
    {
        isPressed.Value = false;
        shrink?.Kill();
        defaultAlpha?.Kill();
        gameObject.SetActive(false);
    }
}
