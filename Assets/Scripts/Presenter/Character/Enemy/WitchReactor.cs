using UnityEngine;


[RequireComponent(typeof(GhostEffect))]
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

        lifeGauge?.OnLifeChange(life, status.LifeMax.Value);
    }

    public void OnResurrection()
    {
        status.ResetStatus();
    }
    public void OnSleep()
    {
        effect.OnDie();
        map.ResetTile();
    }
}
