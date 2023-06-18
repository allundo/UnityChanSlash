using UnityEngine;
using System.Collections.Generic;

public class ResourceFX
{
    private Transform parent;
    private Dictionary<VFXType, ParticleSystem> vfxSources = new Dictionary<VFXType, ParticleSystem>();
    private Dictionary<SNDType, AudioSource> sndSources = new Dictionary<SNDType, AudioSource>();

    public ResourceFX(Transform parent = null)
    {
        this.parent = parent;
    }

    public void PlayVFX(VFXType type) => PlayVFX(type, parent.position);
    public void PlayVFX(VFXType type, Vector3 pos)
    {
        var vfx = vfxSources.LazyLoad(type, type => ResourceLoader.Instance.LoadVFX(type, parent));
        if (vfx == null) return;

        vfx.transform.position = pos;
        vfx.Play();
    }

    public void StopVFX(VFXType type)
    {
        ParticleSystem vfx;
        if (vfxSources.TryGetValue(type, out vfx)) vfx.StopEmitting();
    }

    public void PlaySnd(SNDType type) => PlaySnd(type, parent.position);
    public void PlaySnd(SNDType type, Vector3 pos)
    {
        var snd = sndSources.LazyLoad(type, type => ResourceLoader.Instance.LoadSnd(type, parent));
        if (snd == null) return;

        snd.transform.position = pos;
        snd.PlayEx();
    }

    public void StopSnd(SNDType type)
    {
        AudioSource snd;
        if (sndSources.TryGetValue(type, out snd)) snd.StopEx();
    }
}
