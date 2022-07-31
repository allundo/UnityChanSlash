using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UniRx;
using System;

public class SDIcon : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Sprite[] sdFaces = default;
    [SerializeField] private SDEmotionFX[] emotionFXs = default;

    private FadeTween fade;
    private UITween uiTween;
    private Image image;

    private bool isActive = false;

    private ISubject<Unit> closeSubject = new Subject<Unit>();
    public IObservable<Unit> CloseSignal => closeSubject;

    private Tween activateTween;
    private Tween inactivateTween;

    private SDEmotionFX currentEmotion = null;

    void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image, 1f, true);
        uiTween = new UITween(gameObject, true);

        fade.SetAlpha(0f);

        gameObject.SetActive(false);
    }

    public void Activate(ActiveMessageData messageData)
    {
        currentEmotion?.Inactivate();

        gameObject.SetActive(true);

        inactivateTween.Kill();

        image.sprite = sdFaces[(int)messageData.face];

        int emotionID = (int)messageData.emotion;
        currentEmotion = emotionID >= 0 ? emotionFXs[emotionID] : null;

        uiTween.ResetPos();

        activateTween = DOTween.Sequence()
            .Join(fade.In(0.25f).SetEase(Ease.OutCubic))
            .Join(uiTween.MoveOffset(new Vector2(-120f, 0f), 0.25f).SetEase(Ease.OutCubic))
            .SetUpdate(true)
            .Play();

        currentEmotion?.Activate();

        isActive = true;
    }

    public void Inactivate()
    {
        if (!isActive) return;

        activateTween.Kill();

        inactivateTween = DOTween.Sequence()
            .Join(fade.Out(0.5f, 0f, null, () => gameObject.SetActive(false), true).SetEase(Ease.Linear))
            .Join(uiTween.MoveBack(0.5f).SetEase(Ease.Linear))
            .SetUpdate(true)
            .Play();

        currentEmotion?.Inactivate();

        isActive = false;
    }

    public void OnPointerEnter(PointerEventData eventData) => closeSubject.OnNext(Unit.Default);
}