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
        image = GetComponent<Image>();
        image.raycastTarget = false;

        handleUIs = new[] { openDoorUI, handleRUI, handleLUI };

        doorFlickR = flickR as DoorFlick;
        doorFlickL = flickL as DoorFlick;
    }
    protected override void Start()
    {
        openFlick.IsPressed.Subscribe(_ => SetPressActive(null, true)).AddTo(this);
        openFlick.IsReleased.Subscribe(_ => SetPressActive(null, false)).AddTo(this);

        base.Start();
    }

    public void Activate(bool isOpen)
    {
        if (isActive && this.isOpen == isOpen) return;

        this.isOpen = isOpen;
        isActive = true;
        gameObject.SetActive(true);

        SetActiveButtons(true);
    }

    protected override void SetActiveButtons(bool isActive, float duration = 0.2f)
    {
        if (isActive)
        {
            ActivateButtons(duration);
        }
        else
        {
            base.SetActiveButtons(false);
        }
    }

    protected void ActivateButtons(float duration = 0.2f)
    {
        openDoorUI.SetActive(!isOpen, duration);
        handleRUI.SetActive(isOpen, duration);
        handleLUI.SetActive(isOpen, duration);
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
