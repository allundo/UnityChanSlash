using UnityEngine;
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
        handleUIs = new[] { openDoorUI, handleRUI, handleLUI };

        doorFlickR = flickR as DoorFlick;
        doorFlickL = flickL as DoorFlick;
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
            handleRUI.Activate();
            handleLUI.Activate();
        }
        else
        {
            openDoorUI.Activate();
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
