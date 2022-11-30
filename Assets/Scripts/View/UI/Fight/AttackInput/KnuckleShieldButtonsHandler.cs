using UnityEngine;

public class KnuckleShieldButtonsHandler : AttackButtonsHandler
{
    [SerializeField] private float centerCircleRadius = 100f;

    protected override bool[,] GetCancelableTable()
    {
        var cancelable = new bool[3, 3];

        // LAttack => Kick, RStraight -> LAttack, RStraight -> Kick are cancelable.
        cancelable[0, 2] = cancelable[1, 0] = cancelable[1, 2] = true;

        return cancelable;
    }

    public override void SetUIRadius(float radius)
    {
        sqrCenterRadius = centerCircleRadius * centerCircleRadius;
    }

    private float sqrCenterRadius;
    private bool InCenterCircle(Vector2 uiPos) => uiPos.sqrMagnitude < sqrCenterRadius;

    private static Vector2 axisLeftDown = new Vector2(-1f, -1f);

    public override AttackButton GetAttack(Vector2 uiPos)
    {
        if (InCenterCircle(uiPos)) return attackButtons[1];

        var angle = Vector2.SignedAngle(axisLeftDown, uiPos);

        if (Mathf.Abs(angle) > 90f) return attackButtons[1];

        return angle < 0f ? attackButtons[0] : attackButtons[2];
    }
}
