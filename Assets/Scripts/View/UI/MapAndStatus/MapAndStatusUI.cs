﻿using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

public class MapAndStatusUI : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected MiniMap miniMap = default;
    [SerializeField] protected StatusUI statusUI = default;
    [SerializeField] protected MapAndStatusFrame frame = default;
    [SerializeField] protected ItemInventory itemInventory = default;
    [SerializeField] private float landscapeSize = 420f;
    [SerializeField] private float portraitSize = 480f;
    [SerializeField] private float expandSize = 960f;

    protected Image image;

    protected Tween activateTween = null;

    protected override void Awake()
    {
        miniMap.InitUISize(landscapeSize, portraitSize, expandSize);
        frame.InitUISize(landscapeSize, portraitSize, expandSize);

        fade = new FadeTween(gameObject, 0.4f, true);
        image = GetComponent<Image>();
        Inactivate();
    }

    void Start()
    {
        ResetOrientation(DeviceOrientation.Portrait);
        frame.Press.Subscribe(_ => ExpandMap()).AddTo(this);

        miniMap.Switch.Subscribe(_ =>
        {
            frame.HideAndShow();
            statusUI.ShowStatus();
        })
        .AddTo(this);

        statusUI.Switch.Subscribe(_ =>
        {
            frame.HideAndShow();
            miniMap.ShowMap();
        })
        .AddTo(this);
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        miniMap.ResetOrientation(orientation);
        statusUI.ResetOrientation(orientation);
        frame.ResetOrientation(orientation);
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;

        if (activateTween != null)
        {
            activateTween.Complete(true);
            return;
        }

        ShrinkMap();

    }

    private void ExpandMap()
    {
        image.raycastTarget = true;
        TimeManager.Instance.Pause(true);
        itemInventory.SetActive(false);
        miniMap.SetEnable(false);
        miniMap.HideButton();

        activateTween =
            DOTween.Sequence()
            .Join(frame.ExpandTween(0.75f))
            .Join(FadeIn(0.75f))
            .AppendCallback(() =>
            {
                miniMap.ExpandMap();
                miniMap.SetEnable(true);
                activateTween = null;
            })
            .SetUpdate(true)
            .Play();
    }

    private void ShrinkMap()
    {
        image.raycastTarget = false;
        miniMap.SetEnable(false);
        miniMap.ShrinkMap();
        itemInventory.SetActive(true);

        DOTween.Sequence()
            .Join(frame.ShrinkTween(0.5f))
            .Join(FadeOut(0.5f))
            .AppendCallback(() =>
            {
                TimeManager.Instance.Resume(true);
                miniMap.SetEnable(true);
                miniMap.ShowButton();
            })
            .SetUpdate(true)
            .Play();
    }
}
