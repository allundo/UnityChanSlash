using UnityEngine;
using DG.Tweening;

public class HealSpritEffect : MonoBehaviour, IBodyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected AudioSource hitSnd = default;

    public void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
         => DOVirtual.DelayedCall(duration, onComplete).Play();

    public virtual void OnActive()
    {
        emitVfx?.Play();
    }

    public virtual void OnDie()
    {
        eraseVfx?.Play();
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        hitVfx?.Play();
        hitSnd.PlayEx();
    }

    public void OnDestroyByReactor() { }
}
