using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using System;
using DG.Tweening;

public class ActiveMessageBox : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private CaptionController caption = default;
    private FadeTween fade;
    private UITween uiTween;
    private Image image;

    private bool isActive = false;

    private ISubject<Unit> closeSubject = new Subject<Unit>();
    public IObservable<Unit> CloseSignal => closeSubject;

    private Tween activateTween;
    private Tween inactivateTween;

    void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image, 0.6f, true);
        uiTween = new UITween(gameObject, true);

        image.fillAmount = 0f;
        fade.SetAlpha(0f);

        gameObject.SetActive(false);
    }

    public void Activate(ActiveMessageData messageData)
    {
        gameObject.SetActive(true);

        activateTween?.Kill();
        inactivateTween.Kill();

        image.fillAmount = 0f;
        fade.Enable();
        uiTween.ResetSize();
        caption.Inactivate();

        activateTween = DOTween.Sequence()
            .AppendInterval(0.05f)
            .InsertCallback(0.1f, () => caption.Activate(messageData))
            .Join(DOVirtual.Float(0f, 1f, 0.15f, value => image.fillAmount = value).SetEase(Ease.OutCubic))
            .Join(fade.In(0.15f, 0f, null, null, false))
            .AppendInterval(2f + messageData.sentence.Length * 0.1f)
            .AppendCallback(() => closeSubject.OnNext(Unit.Default))
            .SetUpdate(true)
            .Play();

        isActive = true;
    }

    public void Inactivate()
    {
        if (!isActive) return;

        activateTween.Kill();

        caption.Inactivate();

        inactivateTween = DOTween.Sequence()
            .Join(fade.Out(0.25f, 0f, null, () => gameObject.SetActive(false), true).SetEase(Ease.OutCubic))
            .Join(uiTween.ResizeY(0f, 0.25f).SetEase(Ease.OutCubic))
            .SetUpdate(true)
            .Play();

        isActive = false;
    }

    public void OnPointerEnter(PointerEventData eventData) => closeSubject.OnNext(Unit.Default);
}