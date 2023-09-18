using UniRx;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StructureHandler : MonoBehaviour
{
    [SerializeField] protected HandleUI structureUI = default;
    [SerializeField] protected StructureFlick structureFlick = default;
    [SerializeField] protected float maxAlpha = 0.8f;

    public IObservable<Fountain> ObserveInspect => structureFlick.UpSubject.Select(_ => fountain);
    public IObservable<Fountain> ObserveGet => structureFlick.DownSubject.Select(_ => fountain);
    public IObservable<bool> ObserveHandOn => structureFlick.IsHandOn;

    protected Fountain fountain;

    // FIXME: alpha is used only for activating timer
    protected float alpha = 0.0f;
    protected bool isActive = false;

    public bool isPressed { get; protected set; } = false;

    protected Image image;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        image.raycastTarget = false;
    }

    protected virtual void Start()
    {
        structureFlick.IsPressed.Subscribe(_ => SetPressActive(true)).AddTo(this);
        structureFlick.IsReleased.Subscribe(_ => SetPressActive(false)).AddTo(this);
        structureFlick.Reactivated.Subscribe(_ => structureUI.Activate()).AddTo(this);

        gameObject.SetActive(false);
        structureUI.SetActive(false);
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

    public void Activate(Fountain fountain)
    {
        this.fountain = fountain;
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        structureUI.SetActive(true);
    }

    public void Inactivate(float duration = 0.2f)
    {
        if (!isActive) return;

        isPressed = false;
        isActive = false;
        structureUI.SetActive(false, duration);
    }
}
