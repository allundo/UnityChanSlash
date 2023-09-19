using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

public class MessageController : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected FadeEnable window = default;
    [SerializeField] protected TextHandler textHandler = default;

    protected Image image;

    protected Tween activateTween = null;

    protected bool isSkipValid = false;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
        image = GetComponent<Image>();
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
        textHandler.EndOfSentence.Subscribe(_ => CloseMessage(isUIVisibleOnCompleted)).AddTo(this);

        activateTween =
            DOTween.Sequence()
            .AppendCallback(() =>
            {
                isSkipValid = false;
                TimeManager.Instance.Pause();
            })
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

    private void CloseMessage(bool isUIVisibleOnCompleted = true)
    {
        FadeOut(0.5f, null, () => TimeManager.Instance.Resume(isUIVisibleOnCompleted)).Play();
        window.FadeOut(0.5f).Play();
    }
}
