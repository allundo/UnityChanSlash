using UniRx;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BoxHandler : MonoBehaviour
{
    [SerializeField] protected HandleUI openUI = default;
    [SerializeField] protected HandleUI closeUI = default;
    [SerializeField] protected BoxFlick openFlick = default;
    [SerializeField] protected BoxFlick closeFlick = default;
    [SerializeField] protected float maxAlpha = 0.8f;

    public IObservable<Unit> ObserveHandle => Observable.Merge(openFlick.UpSubject, closeFlick.DownSubject);
    public IObservable<bool> ObserveHandOn => Observable.Merge(openFlick.IsHandOn, closeFlick.IsHandOn);

    // FIXME: alpha is used only for activating timer
    protected float alpha = 0.0f;
    protected bool isActive = false;
    private bool isOpen = false;

    public bool isPressed { get; protected set; } = false;

    protected Image image;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        image.raycastTarget = false;
    }

    protected virtual void Start()
    {
        openFlick.IsPressed.Subscribe(_ => SetPressActive(true)).AddTo(this);
        closeFlick.IsPressed.Subscribe(_ => SetPressActive(true)).AddTo(this);

        openFlick.IsReleased.Subscribe(isApplied => SetPressActive(false)).AddTo(this);
        closeFlick.IsReleased.Subscribe(isApplied => SetPressActive(false)).AddTo(this);

        gameObject.SetActive(false);
        SetActiveButtons(false);
    }

    protected void SetPressActive(bool isActive)
    {
        image.raycastTarget = isActive;
        isPressed = isActive;
    }

    protected virtual void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += (isActive ? 3f : -6f) * Time.deltaTime;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
            return;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;

            gameObject.SetActive(false);
            return;
        }
    }

    public void Activate(bool isOpen)
    {
        if (isActive && this.isOpen == isOpen) return;

        this.isOpen = isOpen;
        isActive = true;
        gameObject.SetActive(true);

        SetActiveButtons(true);
    }

    protected void SetActiveButtons(bool isActive, float duration = 0.2f)
    {
        openUI.SetActive(isActive && !isOpen, duration);
        closeUI.SetActive(isActive && isOpen, duration);
    }

    public void Inactivate(float duration = 0.2f)
    {
        if (!isActive) return;

        isPressed = false;
        isActive = false;
        SetActiveButtons(false, duration);
    }

    public virtual void SetActive(bool value, bool isOpen)
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