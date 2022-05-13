﻿using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;
using System.Linq;

public class MessageController : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected FadeEnable window = default;
    [SerializeField] protected TextHandler textHandler = default;

    protected bool isUIActive = false;

    protected Tween activateTween = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
        Inactivate();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;

        activateTween?.Complete(true);

        textHandler.TapNext();
    }

    public void InputMessageData(MessageData[] data)
    {
        textHandler.Sentence.Subscribe(null, CloseMessage).AddTo(this);

        activateTween =
            DOTween.Sequence()
            .Join(
                FadeIn(
                    0.5f,
                    () => GameManager.Instance.Pause(),
                    () =>
                    {
                        textHandler.InputMessageData(data);
                        activateTween = null;
                    }
                )
            )
            .Join(window.FadeIn(0.5f))
            .SetUpdate(true)
            .Play();
    }

    private void CloseMessage()
    {
        FadeOut(0.5f, null, () => GameManager.Instance.Resume()).Play();
        window.FadeOut(0.5f).Play();
    }
}
