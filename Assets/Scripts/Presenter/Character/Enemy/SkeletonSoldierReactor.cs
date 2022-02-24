using UnityEngine;

[RequireComponent(typeof(UndeadStatus))]
[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(SkeletonSoldierAnimFX))]
public class SkeletonSoldierReactor : ShieldEnemyReactor, IUndeadReactor
{
    protected IUndeadStatus undeadStatus;
    protected IUndeadInput undeadInput;

    protected override void Awake()
    {
        base.Awake();
        undeadStatus = status as IUndeadStatus;
        undeadInput = input as IUndeadInput;
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
        }
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
}
