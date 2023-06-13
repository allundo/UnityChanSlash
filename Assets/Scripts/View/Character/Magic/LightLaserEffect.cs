using UnityEngine;

public class LightLaserEffect : MagicEffectFX
{
    [SerializeField] protected ParticleSystem laserVfx = default;
    [SerializeField] protected ParticleSystem wallVfx = default;
    [SerializeField] protected AudioSource laserSnd = default;

    protected override void OnDisappear()
    {
        wallVfx.StopEmitting();
        laserVfx.StopEmitting();
        laserSnd.StopEx();
    }

    public override void OnActive()
    {
        wallVfx.PlayEx();
        laserVfx.PlayEx();
        laserSnd.PlayEx();
    }
}
