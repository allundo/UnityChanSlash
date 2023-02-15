using UnityEngine;

public interface ITargetUI : IFadeUI
{
    void SetPointerOn();
    void SetPointerOff();
}

public class Target : FadeUI, ITargetUI
{
    [SerializeField] TargetCenter center = default;
    [SerializeField] TargetCorner corner = default;
    [SerializeField] ParticleSystem pointerOnFX = default;
    [SerializeField] float bullEyeRadius = 25f;
    [SerializeField] float activeBullEyeRadius = 60f;
    [SerializeField] TargetName targetName = default;
    [SerializeField] private AudioSource lockOnSnd = default;

    private IEnemyStatus status;
    private RectTransform rectTransform;
    public Vector2 ScreenPos => rectTransform.position;

    public void SetTargetPosOffsetX(float offsetX)
    {
        offset = new Vector3(offsetX, 0f, 0f);
    }
    private Vector3 offset = Vector3.zero;

    protected virtual Camera MainCamera => Camera.main;

    public bool isPointerOn { get; private set; }
    private bool IsPointerOn(Vector2 pointerPos)
    {
        return (ScreenPos - pointerPos).sqrMagnitude < (isPointerOn ? sqrActiveBullEyeRadius : sqrBullEyeRadius);
    }

    private float sqrBullEyeRadius;
    private float sqrActiveBullEyeRadius;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        pointerOnFX.StopAndClear();
        isPointerOn = false;
        sqrBullEyeRadius = bullEyeRadius * bullEyeRadius;
        sqrActiveBullEyeRadius = activeBullEyeRadius * activeBullEyeRadius;
    }

    void Update()
    {
        if (status != null && isActive)
        {
            if (!status.IsAlive)
            {
                status.SetTarget(false);
                FadeInactivate();
                return;
            }
            rectTransform.position = MainCamera.WorldToScreenPoint(status.corePos) + offset;
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
        targetName.Activate(status.NameLv);
    }

    protected override void BeforeFadeOut()
    {
        corner.FadeInactivate();
        center.FadeInactivate();
        targetName.Inactivate();
    }

    protected override void OnDisable()
    {
        corner.Disable();
        center.Disable();
        status = null;
    }

    public void SetPointerOn()
    {
        corner.SetPointerOn();
        center.SetPointerOn();
        pointerOnFX.Play();
    }

    public void SetPointerOff()
    {
        corner.SetPointerOff();
        center.SetPointerOff();
        pointerOnFX.StopAndClear();
    }

    public void SetPointer(Vector2 pointerPos)
    {
        if (!isActive) return;

        if (IsPointerOn(pointerPos))
        {
            if (!isPointerOn) lockOnSnd.PlayEx();
            isPointerOn = true;
            SetPointerOn();
        }
        else
        {
            isPointerOn = false;
            SetPointerOff();
        }
    }
}