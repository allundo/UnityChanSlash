using UnityEngine;

[RequireComponent(typeof(HidePlateUpdater))]
public class PlayerStatus : MobStatus
{
    protected HidePlateUpdater hidePlateUpdater;

    protected override void Awake()
    {
        base.Awake();

        hidePlateUpdater = GetComponent<HidePlateUpdater>();
    }

    public override void Activate()
    {
        ResetStatus();
        map.SetPosition(GameManager.Instance.GetPlayerInitPos, new South());
        hidePlateUpdater.Init();
    }

}
