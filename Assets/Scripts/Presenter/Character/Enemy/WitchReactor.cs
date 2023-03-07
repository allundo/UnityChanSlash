using UnityEngine;

[RequireComponent(typeof(WitchEffect))]
[RequireComponent(typeof(WitchStatus))]
public class WitchReactor : GhostReactor, IMagicianReactor, IUndeadReactor
{
    protected WitchAIInput witchInput;
    protected WitchEffect witchEffect;
    protected Summoner summoner;
    protected UndeadReactor undeadReact;

    protected override void Awake()
    {
        base.Awake();
        witchInput = input as WitchAIInput;
        witchEffect = effect as WitchEffect;
        summoner = new Summoner(map);
        undeadReact = new UndeadReactor(status, input, effect, map, bodyCollider);
    }

    protected override bool CheckAlive(float life)
    {
        if (life > 0f) return true;
        witchInput.InterruptSleep();
        return false;
    }

    protected override void OnActive(EnemyStatus.ActivateOption option)
    {
        Subscribe();
        undeadReact.OnActive(option);
        if (option.isHidden) Hide();
    }

    public void OnResurrection() => undeadReact.OnResurrection();
    public void OnSleep() => undeadReact.OnSleep();

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
