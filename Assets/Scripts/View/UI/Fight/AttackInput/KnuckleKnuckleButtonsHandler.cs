using UnityEngine;

public class KnuckleKnuckleButtonsHandler : AttackButtonsHandler
{
    [SerializeField] private float kickRadius = 100f;

    protected override bool[,] GetCancelableTable()
    {
        var cancelable = new bool[3, 3];

        // Jab -> Straight, Jab => Kick , Straight -> Jab, Straight -> Kick are cancelable.
        cancelable[0, 1] = cancelable[0, 2] = cancelable[1, 0] = cancelable[1, 2] = true;

        return cancelable;
    }

    private Vector2 kickUICenter;
    public override void SetUIRadius(float radius)
    {
        kickUICenter = new Vector2(0, -(radius - kickRadius));
        sqrKickRadius = kickRadius * kickRadius;
    }

    private float sqrKickRadius;
    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).sqrMagnitude < sqrKickRadius;

    public override AttackButton GetAttack(Vector2 uiPos)
    {
        if (InKick(uiPos)) return attackButtons[2];
        return uiPos.x <= 0.0f ? attackButtons[0] : attackButtons[1];
    }
}
