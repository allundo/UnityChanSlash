using UnityEngine;

[RequireComponent(typeof(UndeadStatus))]
[RequireComponent(typeof(UndeadEffect))]
[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(ShieldAnimFX))]
public class SkeletonSoldierReactor : ShieldEnemyReactor, IUndeadReactor
{
    protected UndeadReactor undeadReact;

    protected override void Awake()
    {
        base.Awake();
        undeadReact = new UndeadReactor(status, input, effect, map, bodyCollider);
    }
    protected override bool CheckAlive(float life) => undeadReact.CheckAlive(life);

    protected override void OnActive(EnemyStatus.ActivateOption option)
    {
        Subscribe();
        undeadReact.OnActive(option);
    }

    public void OnResurrection() => undeadReact.OnResurrection();
    public void OnSleep() => undeadReact.OnSleep();
}
