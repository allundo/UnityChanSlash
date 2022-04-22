
public class GhostStatus : EnemyStatus
{
    public override void SetHidden(bool isHidden = true)
    {
        this.isHidden = isHidden;
        this.isOnGround = !isHidden;
    }
}
