using UniRx;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RestButton : Button, IPointerDownHandler, IPointerUpHandler
{
    private Image buttonImage;
    private TextMeshProUGUI textRest;
    private RaycastHandler raycastHandler;
    private RectTransform rectTransform;

    private ISubject<Unit> clickSubject = new Subject<Unit>();
    public IObservable<Unit> Click => clickSubject;

    protected override void Awake()
    {
        base.Awake();
        buttonImage = GetComponent<Image>();
        textRest = GetComponentInChildren<TextMeshProUGUI>();
        raycastHandler = new RaycastHandler(gameObject);
        rectTransform = GetComponent<RectTransform>();
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rectTransform.anchoredPosition = -rectTransform.sizeDelta * 0.5f - new Vector2(20f, 20f + Screen.height * ThirdPersonCamera.Margin);
                break;

            case DeviceOrientation.LandscapeRight:

                rectTransform.anchoredPosition = -rectTransform.sizeDelta * 0.5f - new Vector2(20f, 10f);
                break;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (interactable)
        {
            base.OnPointerDown(eventData);
            return;
        }

        raycastHandler.RaycastPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (interactable)
        {
            base.OnPointerUp(eventData);
            clickSubject.OnNext(Unit.Default);
            return;
        }

        raycastHandler.RaycastPointerUp(eventData);
    }

    public void SetEnable(bool isEnable)
    {
        enabled = isEnable;
        var iColor = buttonImage.color;
        var tColor = textRest.color;
        buttonImage.color = new Color(iColor.r, iColor.g, iColor.b, isEnable ? 1f : 0.5f);
        textRest.color = new Color(tColor.r, tColor.g, tColor.b, isEnable ? 1f : 0.5f);
    }
}
