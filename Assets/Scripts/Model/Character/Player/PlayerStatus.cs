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

    public override void OnActive()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        ResetStatus();
        map.SetPosition(GameManager.Instance.GetPlayerInitPos, new South());
        hidePlateHandler.Init();
    }
}
