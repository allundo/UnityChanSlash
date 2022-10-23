using UnityEngine;

public class SwordKnuckleButtonsHandler : AttackButtonsHandler
{
    [SerializeField] private float stingRadius = 100f;
    [SerializeField] private float chopRadius = 100f;

    public override void SetCommands(PlayerCommandTarget target)
    {
        attackCmds[0] = new PlayerRSlash(target);
        attackCmds[1] = new PlayerLSlash(target);
        attackCmds[2] = new PlayerSting(target);
        attackCmds[3] = new PlayerChop(target);

        criticalCmds[0] = new PlayerRSlashCritical(target);
        criticalCmds[1] = new PlayerLSlashCritical(target);
        criticalCmds[2] = new PlayerStingCritical(target);
        criticalCmds[3] = new PlayerChopCritical(target);
    }

    protected override bool[,] GetCancelableTable()
    {
        var cancelable = new bool[4, 4];

        // RSlash -> LSlash, LSlash => Sting , LSlash -> Chop are cancelable.
        cancelable[0, 1] = cancelable[1, 2] = cancelable[1, 3] = true;

        return cancelable;
    }

    private Vector2 stingUICenter;
    private Vector2 chopUICenter;
    public override void SetUIRadius(float radius)
    {
        stingUICenter = new Vector2(0, -(radius - stingRadius));
        chopUICenter = new Vector2(0, radius - chopRadius);

        sqrStingRadius = stingRadius * stingRadius;
        sqrChopRadius = chopRadius * chopRadius;
    }

    private float sqrStingRadius;
    private bool InSting(Vector2 uiPos) => (stingUICenter - uiPos).sqrMagnitude < sqrStingRadius;
    private float sqrChopRadius;
    private bool InChop(Vector2 uiPos) => (chopUICenter - uiPos).sqrMagnitude < sqrChopRadius;

    public override AttackButton GetAttack(Vector2 uiPos)
    {
        if (InSting(uiPos)) return attackButtons[2];
        if (InChop(uiPos)) return attackButtons[3];
        return uiPos.x >= 0.0f ? attackButtons[0] : attackButtons[1];
    }
}
