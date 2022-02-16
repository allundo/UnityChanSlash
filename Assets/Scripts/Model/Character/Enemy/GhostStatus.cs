public interface IGhostStatus : IEnemyStatus
{
    void SetOnGround(bool isOnGround);
}

public class GhostStatus : EnemyStatus, IGhostStatus
{
    public void SetOnGround(bool isOnGround)
    {
        this.isOnGround = isOnGround;
    }
}
