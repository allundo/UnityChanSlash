using UnityEngine;

[RequireComponent(typeof(SkeletonWizEffect))]
[RequireComponent(typeof(UndeadStatus))]
public class SkeletonWizReactor : EnemyReactor, IMagicianReactor, IUndeadReactor
{
    protected IUndeadInput undeadInput;
    protected IUndeadStatus undeadStatus;
    protected SkeletonWizEffect skeletonWizEffect;
    protected UndeadReactor undeadReact;

    protected override void Awake()
    {
        base.Awake();
        undeadStatus = status as IUndeadStatus;
        undeadInput = input as IUndeadInput;
        skeletonWizEffect = effect as SkeletonWizEffect;
        undeadReact = new UndeadReactor(status, input, effect, map, bodyCollider);
    }

    protected override bool CheckAlive(float life) => undeadReact.CheckAlive(life);

    protected override void OnActive(EnemyStatus.ActivateOption option)
    {
        Subscribe();
        undeadReact.OnActive(option);
    }

    public void OnResurrection() => undeadReact.OnResurrection();
    public void OnSleep() => undeadReact.OnSleep(lastAttacker as IGetExp, ExpObtain);

    public void OnTeleport(float duration)
    {
        skeletonWizEffect.Teleport(duration);
        skeletonWizEffect.TeleportFX();
    }

    public void OnTeleportDest() => skeletonWizEffect.TeleportDestFX();
}
