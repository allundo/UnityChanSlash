using DG.Tweening;
using UnityEngine;

public class Target : FadeActivate
{
    [SerializeField] TargetCenter center = default;
    [SerializeField] TargetCorner corner = default;
    [SerializeField] ParticleSystem pointerOnFX = default;
    [SerializeField] float bullEyeRadius = 25f;
    [SerializeField] float activeBullEyeRadius = 60f;
    [SerializeField] TargetName targetName = default;

    private IEnemyStatus status;
    private RectTransform rectTransform;
    public Vector2 ScreenPos => rectTransform.position;

    protected virtual Camera MainCamera => Camera.main;

    public bool isPointerOn { get; private set; }
    private bool IsPointerOn(Vector2 pointerPos)
    {
        isPointerOn = (ScreenPos - pointerPos).sqrMagnitude < (isPointerOn ? sqrActiveBullEyeRadius : sqrBullEyeRadius);
        return isPointerOn;
    }

    private float sqrBullEyeRadius;
    private float sqrActiveBullEyeRadius;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.2f);
        rectTransform = GetComponent<RectTransform>();
        pointerOnFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        isPointerOn = false;
        sqrBullEyeRadius = bullEyeRadius * bullEyeRadius;
        sqrActiveBullEyeRadius = activeBullEyeRadius * activeBullEyeRadius;
        Inactivate();
    }

    void Update()
    {
        if (status != null)
        {
            rectTransform.position = MainCamera.WorldToScreenPoint(status.corePos);
        }
    }

    public void FadeActivate(IEnemyStatus status)
    {
        isPointerOn = false;
        this.status = status;
        FadeIn(0.5f).Play();
    }

    protected override void OnFadeIn()
    {
        corner.FadeActivate();
        center.FadeActivate();
        targetName.Activate(status.Name);
    }

    public void FadeInactivate()
    {
        FadeOut(0.1f).Play();
    }

    protected override void OnFadeOut()
    {
        corner.FadeInactivate();
        center.FadeInactivate();
        targetName.Inactivate();
    }

    public void SetPointer(Vector2 pointerPos)
    {
        if (!isActive) return;

        if (IsPointerOn(pointerPos))
        {
            corner.SetPointerOn();
            center.SetPointerOn();
            pointerOnFX.Play();
        }
        else
        {
            corner.SetPointerOff();
            center.SetPointerOff();
            pointerOnFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}