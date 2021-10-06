using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

[RequireComponent(typeof(Image))]
public class BaseHandler : MonoBehaviour
{
    [SerializeField] protected HandleUI handleRUI = default;
    [SerializeField] protected HandleUI handleLUI = default;
    [SerializeField] protected FlickInteraction flickR = default;
    [SerializeField] protected FlickInteraction flickL = default;
    [SerializeField] protected float maxAlpha = 0.8f;

    protected IObservable<Unit> ObserveUp => Observable.Merge(flickR.UpSubject, flickL.UpSubject);
    protected IObservable<Unit> ObserveDown => Observable.Merge(flickR.DownSubject, flickL.DownSubject);
    protected IObservable<Unit> ObserveRL => Observable.Merge(flickR.LeftSubject, flickL.RightSubject);

    // FIXME: alpha is used only for activating timer
    protected float alpha = 0.0f;
    protected bool isActive = false;
    public bool isPressed { get; protected set; } = false;

    protected Image image;

    protected HandleUI[] handleUIs;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        image.raycastTarget = false;

        handleUIs = new[] { handleRUI, handleLUI };
    }

    protected virtual void Start()
    {
        flickL.IsPressed.Subscribe(_ => SetPressActive(handleRUI, true)).AddTo(this);
        flickR.IsPressed.Subscribe(_ => SetPressActive(handleLUI, true)).AddTo(this);

        flickL.IsReleased.Subscribe(_ => SetPressActive(handleRUI, false)).AddTo(this);
        flickR.IsReleased.Subscribe(_ => SetPressActive(handleLUI, false)).AddTo(this);

        gameObject.SetActive(false);
        SetActiveButtons(false);
    }

    protected void SetPressActive(HandleUI handleUI, bool isActive)
    {
        image.raycastTarget = isActive;
        isPressed = isActive;
        handleUI?.SetActive(!isActive);
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

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        SetActiveButtons(true);
    }

    protected virtual void SetActiveButtons(bool isActive, float duration = 0.2f)
    {
        handleUIs.ForEach(handleUI => handleUI.SetActive(isActive, duration));
    }

    public void Inactivate(float duration = 0.2f)
    {
        if (!isActive) return;

        isPressed = false;
        isActive = false;
        SetActiveButtons(false, duration);
    }

    public virtual void SetActive(bool value)
    {
        if (value)
        {
            Activate();
        }
        else
        {
            Inactivate();
        }
    }
}
