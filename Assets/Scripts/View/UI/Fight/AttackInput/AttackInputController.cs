using UnityEngine;
using DG.Tweening;
using UniRx;

public class AttackInputController : MonoBehaviour
{
    private AttackButtonsHandler attackButtonsHandler = null;

    [SerializeField] protected Target enemyTarget = default;
    [SerializeField] private PivotPoint pivotPoint = default;
    [SerializeField] private EffortPoint effortPoint = default;
    [SerializeField] private TargetPointer targetPointer = default;

    [SerializeField] private float attackCancelThreshold = 2.0f;

    public Target GetEnemyTarget() => enemyTarget;

    private Vector2 UICenter;

    public void SetUICenter(Vector2 pos) => UICenter = pos;
    public void SetUIRadius(float radius)
    {
        this.radius = radius;
        sqrRadius = radius * radius;
    }

    public AttackButtonsHandler SetAttackButtonsHandler(IEquipmentStyle equipments)
    {
        if (attackButtonsHandler != null) Destroy(attackButtonsHandler.gameObject);

        attackButtonsHandler = equipments.LoadAttackButtonsHandler(transform);
        attackButtonsHandler.SetTarget(enemyTarget);
        attackButtonsHandler.SetUIRadius(radius);
        return attackButtonsHandler;
    }

    private AttackButton currentButton = null;

    public Vector2 pressPos { get; private set; } = Vector2.zero;
    public bool IsPressed => currentButton != null;

    private float radius;
    private float sqrRadius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - UICenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    public bool IsDragCancel(Vector2 screenPos) => DrawComponent(screenPos) < -attackCancelThreshold;

    public bool InCircle(Vector2 screenPos) => UIPos(screenPos).sqrMagnitude < sqrRadius;

    public IReadOnlyReactiveProperty<bool> IsChargingUp => isChargingUp;
    private IReactiveProperty<bool> isChargingUp = new ReactiveProperty<bool>(false);
    private Vector2 pointerVec = Vector2.zero;

    void Update()
    {
        if (isChargingUp.Value) enemyTarget.SetPointer(pressPos + pointerVec);
    }

    public void Release()
    {
        currentButton?.Release();
        currentButton = null;

        pivotPoint.Hide();
        effortPoint.Hide();
        targetPointer.Hide();

        isChargingUp.Value = false;
        enemyTarget.SetPointer(Vector2.zero);
    }

    public void Press(Vector2 screenPos)
    {
        pressPos = screenPos;
        currentButton = attackButtonsHandler.GetAttack(UIPos(screenPos));

        currentButton.Press(pressPos);
    }

    public void ChargeUp(Vector2 screenPos)
    {
        pivotPoint.Show(pressPos);
        effortPoint.Show(screenPos);
        targetPointer.Show(pressPos);

        pointerVec = pressPos - screenPos;
        targetPointer.SetVerticesPos(pointerVec);

        isChargingUp.Value = !InCircle(screenPos);

        if (isChargingUp.Value)
        {
            pivotPoint.EnableChargingUp();
            effortPoint.EnableChargingUp();
            targetPointer.EnableChargingUp();
        }
        else
        {
            pivotPoint.DisableChargingUp();
            effortPoint.DisableChargingUp();
            targetPointer.DisableChargingUp();
        }
    }

    public void ButtonCancel(bool isFadeOnly = false)
    {
        if (isFadeOnly)
        {
            currentButton?.FadeOut()?.Play();
        }
        else
        {
            currentButton?.Cancel();
        }

        pivotPoint.Hide();
        effortPoint.Hide();
        targetPointer.Hide();

        isChargingUp.Value = false;

        currentButton = null;
        pressPos = Vector2.zero;
    }
}
