using UnityEngine;

public class SkeletonSoldierAnimFX : ShieldEnemyAnimFX
{
    [SerializeField] private AudioSource resurrectionSfx = null;
    [SerializeField] protected ParticleSystem resurrectionVfx = default;


    // Called as Animation Event functions
    public virtual void OnResurrection() => Play(resurrectionSfx, resurrectionVfx);
}