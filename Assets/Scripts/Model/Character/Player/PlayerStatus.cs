using UnityEngine;

public class PlayerStatus : MobStatus
{
    [SerializeField] protected MobData data;

    protected override void Awake()
    {
        base.Awake();
        InitParam(data.Param(0));
    }

    public void SetPosition() => (map as PlayerMapUtil).SetPosition();
    public void SetPosition(bool isDownStairs) => (map as PlayerMapUtil).SetPosition(isDownStairs);

}
