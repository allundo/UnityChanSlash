using UnityEngine;

public class Target : FadeUI
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
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        pointerOnFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        isPointerOn = false;
        sqrBullEyeRadius = bullEyeRadius * bullEyeRadius;
        sqrActiveBullEyeRadius = activeBullEyeRadius * activeBullEyeRadius;
    }

    void Update()
    {
        if (status != null)
        {
            rectTransform.position = MainCamera.WorldToScreenPoint(status.corePos);
        }
    }

    public void FadeActivate(IEnemyStatus status, float duration = 0.5f)
    {
        isPointerOn = false;
        this.status = status;
        base.FadeActivate(duration);
    }

    protected override void OnFadeEnable(float fadeDuration)
    {
        corner.FadeActivate();
        center.FadeActivate();
        targetName.Activate(status.Name);
    }

    protected override void BeforeFadeOut()
    {
        corner.FadeInactivate();
        center.FadeInactivate();
        targetName.Inactivate();
    }

    public override void SetPointerOn()
    {
        corner.SetPointerOn();
        center.SetPointerOn();
        pointerOnFX.Play();
    }

    public override void SetPointerOff()
    {
        corner.SetPointerOff();
        center.SetPointerOff();
        pointerOnFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetPointer(Vector2 pointerPos)
    {
        if (!isActive) return;

        if (IsPointerOn(pointerPos))
        {
            SetPointerOn();
        }
        else
        {
            SetPointerOff();
        }
    }
}