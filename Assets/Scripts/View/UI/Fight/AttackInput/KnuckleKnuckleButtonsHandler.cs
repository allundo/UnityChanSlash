using UnityEngine;

public class KnuckleKnuckleButtonsHandler : AttackButtonsHandler
{
    [SerializeField] private float kickRadius = 100f;

    public override void SetCommands(PlayerCommandTarget target)
    {
        attackCmds[0] = new PlayerJab(target, 21.6f);
        attackCmds[1] = new PlayerStraight(target, 30f);
        attackCmds[2] = new PlayerKick(target, 43f);

        criticalCmds[0] = new PlayerJabCritical(target, 18.5f);
        criticalCmds[1] = new PlayerStraightCritical(target, 24f);
        criticalCmds[2] = new PlayerKickCritical(target, 35f);
    }

    protected override bool[,] GetCancelableTable()
    {
        var cancelable = new bool[3, 3];

        // Jab -> Straight, Jab => Kick , Straight -> Jab are cancelable.
        cancelable[0, 1] = cancelable[0, 2] = cancelable[1, 0] = true;

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
