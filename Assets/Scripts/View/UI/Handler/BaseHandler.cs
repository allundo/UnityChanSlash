using UnityEngine;
using System;
using UniRx;
using DG.Tweening;

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

    protected float alpha = 0.0f;
    protected bool isActive = false;
    public bool isPressed { get; protected set; } = false;

    protected HandleUI[] handleUIs;
    private FlickInteraction currentFlick = null;

    protected virtual void Awake()
    {
        handleUIs = new[] { handleRUI, handleLUI };
    }

    protected virtual void Start()
    {
        flickL.IsPressed.Subscribe(_ => { handleRUI.Inactivate(); isPressed = true; }).AddTo(this);
        flickR.IsPressed.Subscribe(_ => { handleLUI.Inactivate(); isPressed = true; }).AddTo(this);

        flickL.ReleaseSubject.Subscribe(_ => { handleRUI.Activate(); isPressed = false; }).AddTo(this);
        flickR.ReleaseSubject.Subscribe(_ => { handleLUI.Activate(); isPressed = false; }).AddTo(this);

        gameObject.SetActive(false);
        InactivateButtons();
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
            InactivateButtons();

            return;
        }
    }

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        ActivateButtons();
    }

    protected virtual void ActivateButtons()
    {
        alpha = 0.0f;

        handleRUI.Activate();
        handleLUI.Activate();
    }

    private void InactivateButtons()
    {
        handleUIs.ForEach(handleUI => handleUI.Inactivate());
        currentFlick?.FadeOut()?.Play();
    }

    public void Inactivate()
    {
        if (!isActive) return;

        ButtonCancel(true);
        isActive = false;
    }

    protected void ButtonCancel(bool isFadeOnly = false)
    {
        if (isFadeOnly)
        {
            currentFlick?.FadeOut()?.Play();
        }
        else
        {
            currentFlick?.Cancel();
        }

        isPressed = false;
        currentFlick = null;
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
