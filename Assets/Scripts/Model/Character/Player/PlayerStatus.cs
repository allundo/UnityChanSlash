public class PlayerStatus : MobStatus
{
    public void SetPosition() => (map as PlayerMapUtil).SetPosition();
    public void SetPosition(bool isDownStairs) => (map as PlayerMapUtil).SetPosition(isDownStairs);
}
