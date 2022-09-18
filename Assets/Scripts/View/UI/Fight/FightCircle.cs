using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

[RequireComponent(typeof(MaskableGraphic))]
public class FightCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private AttackInputController attackInputUI = default;
    [SerializeField] private float maxAlpha = 1f;

    [SerializeField] private EnemyLifeGauge circle = default;
    [SerializeField] private Target enemyTarget = default;

    [SerializeField] private RectTransform forwardUIRT = default;
    [SerializeField] private float radius = 260f;
    [SerializeField] private float forwardRadius = 80f;

    private RectTransform rectTransform;
    private RaycastHandler raycastHandler;

    private float alpha = 0.0f;
    public bool isActive { get; private set; } = false;
    private PointerEventData pointerDownEventData = null;

    public bool isForwardMovable { private get; set; } = false;
    private Vector2 forwardUIPos;
    private float sqrForwardRadius;
    private bool InForward(Vector2 screenPos) => (forwardUIPos - screenPos).sqrMagnitude < sqrForwardRadius;

    private IReactiveProperty<IEnemyStatus> EnemyStatus = new ReactiveProperty<IEnemyStatus>(null);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        raycastHandler = new RaycastHandler(gameObject);
    }

    void Start()
    {
        sqrForwardRadius = forwardRadius * forwardRadius;

        ResetCenterPos();

        attackInputUI.SetUIRadius(radius);

        circle.SetAlpha(0.0f);
        gameObject.SetActive(false);

        EnemyStatus
            .Where(status => status != null)
            .SelectMany(status =>
            {
                enemyTarget.FadeActivate(status);
                circle.OnEnemyChange(status.Life.Value, status.LifeMax.Value);
                return status.Life;
            })
            .TakeUntil(EnemyStatus.Skip(1))
            .RepeatUntilDestroy(gameObject)
            .Subscribe(life => circle.OnLifeChange(life))
            .AddTo(this);
    }

    void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += (isActive ? 6f : -6f) * Time.deltaTime;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;
            gameObject.SetActive(false);
        }

        circle.SetAlpha(alpha);
    }

    public void ResetCenterPos()
    {
        forwardUIPos = forwardUIRT.position;
        attackInputUI.SetUICenter(rectTransform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDownEventData = null;

        if (!attackInputUI.IsPressed)
        {
            if (!attackInputUI.InCircle(eventData.position))
            {
                raycastHandler.RaycastPointerUp(eventData);
                return;
            }

            if (isForwardMovable && InForward(eventData.position))
            {
                raycastHandler.RaycastPointerExit(eventData);
                return;
            }
        }

        if (!isActive) return;

        attackInputUI.Release();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownEventData = eventData;

        if (!attackInputUI.InCircle(eventData.position))
        {
            raycastHandler.RaycastPointerDown(eventData);
            return;
        }

        if (isForwardMovable && InForward(eventData.position))
        {
            raycastHandler.RaycastPointerEnter(eventData);
            return;
        }

        if (!isActive) return;

        attackInputUI.Press(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (attackInputUI.IsDragCancel(eventData.position))
        {
            ButtonCancel();
            return;
        }

        if (!attackInputUI.IsPressed)
        {
            raycastHandler.RaycastDrag(eventData);
            return;
        }

        attackInputUI.ChargeUp(eventData.position);
    }

    public void Activate(IEnemyStatus status)
    {
        EnemyStatus.Value = status;

        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
    }

    public void Inactivate(bool isForce = false)
    {
        if (!isForce && attackInputUI.IsPressed || !isActive) return;

        ButtonCancel(true);
        isActive = false;
        EnemyStatus.Value = null;
        enemyTarget.FadeInactivate();
    }

    private void ButtonCancel(bool isFadeOnly = false)
    {
        attackInputUI.ButtonCancel(isFadeOnly);

        enemyTarget.SetPointer(Vector2.zero);

        if (pointerDownEventData != null)
        {
            raycastHandler.RaycastPointerUp(pointerDownEventData, true);
        }
    }

    public void SetActive(bool value, IEnemyStatus status)
    {
        if (value)
        {
            Activate(status);
        }
        else
        {
            Inactivate();
        }
    }
}
