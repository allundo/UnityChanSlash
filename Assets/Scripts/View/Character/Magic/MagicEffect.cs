using UnityEngine;
using DG.Tweening;

public interface IMagicEffect : IEffect
{
    void OnHit();
}

public class MagicEffect : MonoBehaviour, IMagicEffect
{
    public virtual void Disappear(TweenCallback onComplete = null, float duration = 0.5F)
    {
        if (onComplete != null) onComplete();
    }
    public virtual void OnActive() { }
    public virtual void OnHit() { }
    public virtual void OnDestroyByReactor() { }
}

public abstract class MagicEffectFX : MagicEffect
{
    protected Tween deadTimer;
    public override void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
    {
        OnDisappear();
        deadTimer = DOVirtual.DelayedCall(duration, onComplete, false).Play();
    }
    protected abstract void OnDisappear();
    public override void OnDestroyByReactor() => deadTimer?.Complete();
}
