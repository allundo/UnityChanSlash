using UnityEngine;


[RequireComponent(typeof(WitchEffect))]
[RequireComponent(typeof(WitchStatus))]
public class WitchReactor : GhostReactor, IUndeadReactor
{
    protected WitchAIInput witchInput;
    protected WitchStatus witchStatus;

    protected override void Awake()
    {
        base.Awake();
        witchStatus = status as WitchStatus;
        witchInput = input as WitchAIInput;
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) witchInput.InputSleep();
    }

    public void OnResurrection()
    {
        status.ResetStatus();
        bodyCollider.enabled = true;
    }
    public void OnSleep()
    {
        effect.OnDie();
        map.ResetTile();
        bodyCollider.enabled = false;
    }

    public void OnTeleport(float duration)
    {
        (effect as WitchEffect).TeleportWipe(duration);
    }
}
