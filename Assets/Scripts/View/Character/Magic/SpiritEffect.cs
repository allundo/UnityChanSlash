using UnityEngine;

public class SpiritEffect : MagicEffectFX
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected AudioSource hitSnd = default;


    public override void OnActive()
    {
        emitVfx.PlayEx();
    }

    protected override void OnDisappear()
    {
        eraseVfx.PlayEx();
        emitVfx.StopEmitting();
    }
    public override void OnHit()
    {
        hitVfx.PlayEx();
        hitSnd.PlayEx();
    }
}
