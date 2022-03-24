using UnityEngine;


[RequireComponent(typeof(WitchEffect))]
[RequireComponent(typeof(WitchStatus))]
public class WitchReactor : GhostReactor, IUndeadReactor
{
    protected WitchAIInput witchInput;
    protected WitchStatus witchStatus;
    protected WitchEffect witchEffect;

    protected override void Awake()
    {
        base.Awake();
        witchStatus = status as WitchStatus;
        witchInput = input as WitchAIInput;
        witchEffect = effect as WitchEffect;
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
        witchEffect.TeleportWipe(duration);
        witchEffect.TeleportFX();
    }

    public void OnTeleportDest()
    {
        witchEffect.TeleportDestFX();
    }

    public void OnTeleportEnd()
    {
        witchEffect.OnTeleportEnd();
    }
}
