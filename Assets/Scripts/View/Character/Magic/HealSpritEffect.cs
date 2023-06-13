using UnityEngine;
using DG.Tweening;

public class HealSpritEffect : MonoBehaviour, IBodyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected AudioSource hitSnd = default;

    private Tween disappearingTween;

    public void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
    {
        disappearingTween = DOVirtual.DelayedCall(duration, onComplete).Play();
    }

    public virtual void OnActive()
    {
        emitVfx.PlayEx();
    }

    public virtual void OnDie()
    {
        eraseVfx.PlayEx();
        emitVfx.StopEmitting();
    }
    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        hitVfx.PlayEx();
        hitSnd.PlayEx();
    }

    public void OnDestroyByReactor()
    {
        disappearingTween?.Complete(true);
    }
}
