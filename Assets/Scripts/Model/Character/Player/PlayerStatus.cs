using UnityEngine;

[RequireComponent(typeof(HidePool))]
public class PlayerStatus : MobStatus
{
    protected HidePool hidePool;

    protected override void Awake()
    {
        base.Awake();

        hidePool = GetComponent<HidePool>();
    }

    public override void Activate()
    {
        ResetStatus();
        map.SetPosition(GameManager.Instance.GetPlayerInitPos, new South());
        hidePool.Init();
    }

}
