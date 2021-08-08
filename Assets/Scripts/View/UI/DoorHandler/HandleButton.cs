﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HandleButton : MonoBehaviour
{
    [SerializeField] private Sprite handle = default;
    [SerializeField] private Sprite circle = default;
    [SerializeField] private float maxAlpha = 1.0f;


    protected RectTransform rectTransform;
    protected Image image;
    private Vector2 defaultSize;

    private Tween cycle;
    private Tween expand;
    private bool isPressed = false;

    public Vector2 Size => rectTransform.sizeDelta;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Start()
    {
        defaultSize = Size;
        cycle = GetRotate(-90.0f, 5.0f, true).SetEase(Ease.Linear);
        expand = GetResize(1.5f, 0.2f, true);

    }

    private void ResetSize()
    {
        rectTransform.sizeDelta = defaultSize;
    }

    private Tween GetRotate(float angle, float duration, bool isLoop = false)
    {
        Tween rotate = rectTransform.DOLocalRotate(new Vector3(0, 0, angle), duration);

        return isLoop ? rotate.SetLoops(-1).AsReusable(gameObject) : rotate;
    }

    private Tween GetResize(float ratio = 1.5f, float duration = 0.2f, bool isReusable = false)
    {
        Tween resize = rectTransform.DOSizeDelta(defaultSize * ratio, duration);

        return isReusable ? resize.AsReusable(gameObject) : resize;
    }

    public void OnDrag(float dragRatio)
    {
        SetAlpha(1.0f - dragRatio);

        if (isPressed) return;

        isPressed = true;
        image.sprite = circle;
        cycle.Restart();
        expand.Restart();
    }

    public void OnRelease()
    {
        isPressed = false;

        cycle.Rewind();
        expand.Rewind();
        ResetSize();
        SetAlpha(1.0f);
        image.sprite = handle;
    }

    public void SetAlpha(float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }

    public void Activate(float alpha)
    {
        isPressed = false;
        SetAlpha(alpha);
        gameObject.SetActive(true);
    }

    public void Inactivate()
    {
        OnRelease();
        gameObject.SetActive(false);
    }
}
