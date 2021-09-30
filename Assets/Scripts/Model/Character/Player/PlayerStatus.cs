using UnityEngine;

[RequireComponent(typeof(HidePlateHandler))]
public class PlayerStatus : MobStatus
{
    protected HidePlateHandler hidePlateHandler;

    protected override void Awake()
    {
        base.Awake();

        hidePlateHandler = GetComponent<HidePlateHandler>();
    }

    public override void Activate()
    {
        ResetStatus();
        map.SetPosition(GameManager.Instance.GetPlayerInitPos, new South());
        hidePlateHandler.Init();
    }

}
