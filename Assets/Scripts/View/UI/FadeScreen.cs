using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private ScreenRotateHandler screenRotate = default;

    private RectTransform rectTransform;
    private Image black;
    private Tween fadeIn = null;
    private Tween fadeOut = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        black = GetComponent<Image>();
    }

    void Start()
    {

        screenRotate.Orientation
            .SkipLatestValueOnSubscribe()
            .Subscribe(orientation => ResetOrientation(orientation))
            .AddTo(this);
    }

    private void ResetOrientation(DeviceOrientation orientation)
    {
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }

    public Tween FadeIn(float duration = 1f)
    {
        fadeIn = DOTween
            .ToAlpha(() => black.color, color => black.color = color, 0f, duration)
            .OnPlay(() => fadeOut?.Kill());

        return fadeIn;
    }

    public Tween FadeOut(float duration = 1f)
    {
        fadeOut = DOTween
            .ToAlpha(() => black.color, color => black.color = color, 1f, duration)
            .OnPlay(() => fadeIn?.Kill());

        return fadeOut;
    }
}
