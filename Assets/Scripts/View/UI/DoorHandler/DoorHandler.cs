using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class DoorHandler : BaseHandler
{
    [SerializeField] private HandleUI openDoorUI = default;
    [SerializeField] private DoorFlick openFlick = default;

    private DoorFlick doorFlickR;
    private DoorFlick doorFlickL;

    public IObservable<Unit> ObserveGo => ObserveUp;
    public IObservable<Unit> ObserveHandle => Observable.Merge(openFlick.RightSubject, openFlick.LeftSubject, ObserveRL);
    public IObservable<bool> ObserveHandOn => Observable.Merge(openFlick.IsHandOn, doorFlickR.IsHandOn, doorFlickL.IsHandOn);

    private bool isOpen = false;

    protected override void Awake()
    {
        rawImage = GetComponent<RawImage>();
        handleUIs = new[] { openDoorUI, handleRUI, handleLUI };

        doorFlickR = flickR as DoorFlick;
        doorFlickL = flickL as DoorFlick;
    }

    protected override void SetAlpha(float alpha)
    {
        if (isOpen)
        {
            base.SetAlpha(alpha);
        }
        else
        {
            openDoorUI.SetAlpha(alpha);
        }
    }

    protected override FlickInteraction GetFlick(Vector2 screenPos)
    {
        if (isOpen)
        {
            return base.GetFlick(screenPos);
        }
        else
        {
            if (openDoorUI.InRegion(screenPos)) return openFlick;
        }

        return null;
    }

    public void Activate(bool isOpen)
    {
        if (isActive && this.isOpen == isOpen) return;

        this.isOpen = isOpen;
        isActive = true;
        gameObject.SetActive(true);

        ActivateButtons();
    }

    protected override void ActivateButtons()
    {
        alpha = 0.0f;

        if (isOpen)
        {
            openDoorUI.Inactivate();
            handleRUI.Activate(alpha);
            handleLUI.Activate(alpha);
        }
        else
        {
            openDoorUI.Activate(alpha);
            handleRUI.Inactivate();
            handleLUI.Inactivate();
        }
    }

    public void SetActive(bool value, bool isOpen)
    {
        if (value)
        {
            Activate(isOpen);
        }
        else
        {
            Inactivate();
        }
    }
}
