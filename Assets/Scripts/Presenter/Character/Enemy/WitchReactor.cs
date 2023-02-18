using UnityEngine;

[RequireComponent(typeof(WitchEffect))]
[RequireComponent(typeof(WitchStatus))]
public class WitchReactor : GhostReactor, IMagicianReactor, IUndeadReactor
{
    protected WitchAIInput witchInput;
    protected WitchStatus witchStatus;
    protected WitchEffect witchEffect;
    protected Summoner summoner;

    protected override void Awake()
    {
        base.Awake();
        witchStatus = status as WitchStatus;
        witchInput = input as WitchAIInput;
        witchEffect = effect as WitchEffect;
        summoner = new Summoner(map);
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f)
        {
            witchInput.InputSleep();
            return;
        }
        if (!enemyStatus.IsTarget.Value) gaugeGenerator.Show(enemyStatus);
    }

    public void OnResurrection()
    {
        status.ResetStatus();
        witchEffect.OnResurrection();
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
    public void OnTeleportDest() => witchEffect.TeleportDestFX();

    public void Summon()
    {
        summoner.SummonMulti(8 - GameInfo.Instance.currentFloor);
    }

    public void OnSummonStart()
    {
        witchEffect.OnSummonStart();
    }

    public bool IsSummoning => summoner.IsSummoning;

    public override void OnOutOfView()
    {
        // Don't disappear. Close to player again.
        witchInput.InputTeleport();
    }

    public override void Destroy()
    {
        summoner.StopSummoning();
        base.Destroy();
    }
}
