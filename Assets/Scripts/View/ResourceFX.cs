using UnityEngine;
using System.Collections.Generic;

public class ResourceFX
{
    protected Dictionary<VFXType, ParticleSystem> bodyVfxSource = new Dictionary<VFXType, ParticleSystem>();
    public void PlayVFX(VFXType type, Vector3 pos)
    {
        var vfx = bodyVfxSource.LazyLoad(type, ResourceLoader.Instance.LoadVFX);
        if (vfx == null) return;

        vfx.transform.position = pos;
        vfx.Play();
    }
    public void StopVFX(VFXType type) => bodyVfxSource.LazyLoad(type, ResourceLoader.Instance.LoadVFX).Stop(true, ParticleSystemStopBehavior.StopEmitting);

    protected Dictionary<SNDType, AudioSource> bodySndSource = new Dictionary<SNDType, AudioSource>();
    public void PlaySnd(SNDType type, Vector3 pos)
    {
        var snd = bodySndSource.LazyLoad(type, ResourceLoader.Instance.LoadSnd);
        if (snd == null) return;

        snd.transform.position = pos;
        snd.PlayEx();
    }
    public void StopSnd(SNDType type) => bodySndSource.LazyLoad(type, ResourceLoader.Instance.LoadSnd).StopEx();
}