using UnityEngine;

[RequireComponent(typeof(SkeletonWizEffect))]
[RequireComponent(typeof(UndeadStatus))]
public class SkeletonWizReactor : EnemyReactor, IMagicianReactor, IUndeadReactor
{
    protected IUndeadInput undeadInput;
    protected IUndeadStatus undeadStatus;
    protected SkeletonWizEffect skeletonWizEffect;

    protected override void Awake()
    {
        base.Awake();
        undeadStatus = status as IUndeadStatus;
        undeadInput = input as IUndeadInput;
        skeletonWizEffect = effect as SkeletonWizEffect;
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f)
        {
            if (undeadStatus.curse > 0f)
            {
                undeadInput.InputSleep();
            }
            else
            {
                input.InputDie();
            }
            return;
        }
        if (!enemyStatus.IsTarget.Value) enemyEffect.ShowGauge(enemyStatus.LifeRatio);
    }

    public void OnResurrection()
    {
        status.ResetStatus();
        skeletonWizEffect.OnResurrection();
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
        skeletonWizEffect.Teleport(duration);
        skeletonWizEffect.TeleportFX();
    }

    public void OnTeleportDest() => skeletonWizEffect.TeleportDestFX();
}
