public class PlayerStatus : MobStatus
{
    public override void SetPosition()
        => (map as PlayerMapUtil).SetPosition();
    public virtual void SetPosition(bool isDownStairs)
        => (map as PlayerMapUtil).SetPosition(isDownStairs);
}
