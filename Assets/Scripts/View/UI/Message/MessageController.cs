using System;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;
using System.Linq;

public class MessageController : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected MessageWindowUI window = default;
    [SerializeField] protected TextHandler textHandler = default;

    protected Tween activateTween = null;

    protected bool isSkipValid = false;

    public IObservable<Unit> OnActive => activeSubject;
    protected ISubject<Unit> activeSubject = new Subject<Unit>();

    public IObservable<bool> OnInactive => inactiveSubject;
    protected ISubject<bool> inactiveSubject = new Subject<bool>();

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
    }

    void Start()
    {

        Inactivate();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive || !isSkipValid) return;

        activateTween?.Complete(true);

        textHandler.TapNext();
    }

    public void InputMessageData(MessageData data, bool isUIVisibleOnCompleted = true)
    {
        textHandler.EndOfSentence
            .First()
            .Subscribe(_ => CloseMessage(isUIVisibleOnCompleted))
            .AddTo(this);

        isSkipValid = false;
        activeSubject.OnNext(Unit.Default);

        activateTween =
            DOTween.Sequence()
            .Join(FadeIn(0.5f))
            .Join(window.FadeIn(0.5f))
            .AppendCallback(() =>
            {
                isSkipValid = true;
                textHandler.InputMessageData(data);
                activateTween = null;
            })
            .SetUpdate(true)
            .Play();
    }

    public void InputPictureMessageData(PictureMessageData data)
    {
        textHandler.EndOfSentence
            .First()
            .ContinueWith(_ =>
            {
                isSkipValid = false;

                activateTween = DOTween.Sequence()
                    .Append(window.FadeOut(0.5f))
                    .AppendCallback(() =>
                    {
                        window.ResetToWindow();

                        activateTween = DOTween.Sequence()
                            .Append(window.FadeIn(0.5f))
                            .AppendCallback(() =>
                            {
                                isSkipValid = true;
                                textHandler.InputMessageData(data);
                                activateTween = null;
                            })
                            .SetUpdate(true)
                            .Play();
                    })
                    .SetUpdate(true)
                    .Play();

                return textHandler.EndOfSentence.First();
            })
            .Subscribe(_ => CloseMessage())
            .AddTo(this);

        isSkipValid = false;
        activeSubject.OnNext(Unit.Default);
        window.SetPicture();

        activateTween =
            DOTween.Sequence()
            .Join(FadeIn(0.5f))
            .Join(window.FadeIn(0.5f))
            .AppendCallback(() =>
            {
                isSkipValid = true;
                textHandler.InputMessageData(new MessageData(new MessageSource(" "))); // Input dummy message to display picture
                activateTween = null;
            })
            .SetUpdate(true)
            .Play();
    }

    private void CloseMessage(bool isUIVisibleOnCompleted = true)
    {
        FadeOut(0.5f, null, () => inactiveSubject.OnNext(isUIVisibleOnCompleted)).Play();
        window.FadeOut(0.5f).Play();
    }
}
