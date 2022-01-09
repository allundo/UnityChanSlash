using UnityEngine;

public class PlayerStatus : MobStatus
{
    [SerializeField] protected MobData data;

    protected override void Awake()
    {
        param = data.Param(0);
        base.Awake();
    }

    public void SetPosition() => (map as PlayerMapUtil).SetPosition();
    public void SetPosition(bool isDownStairs) => (map as PlayerMapUtil).SetPosition(isDownStairs);

}
