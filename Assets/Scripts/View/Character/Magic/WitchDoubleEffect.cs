using UnityEngine;
using DG.Tweening;

public class WitchDoubleEffect : MagicEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected Transform meshTf = null;

    protected MatColorEffect witchMatEffect;
    protected virtual void Awake()
    {
        witchMatEffect = new MatColorEffect(meshTf);
    }

    public override void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
        => witchMatEffect.Inactivate(onComplete, duration);

    public void OnAttackStart()
    {
        emitVfx.PlayEx();
    }

    public void OnAttackEnd()
    {
        emitVfx.StopEmitting();
    }

    public override void OnActive()
    {
        witchMatEffect.Activate(0.01f);
    }

    public override void OnDestroyByReactor() => witchMatEffect.KillAllTweens();
}
