using UnityEngine;

static public class ParticleSystemExtensions
{
    static public void PlayEx(this ParticleSystem source)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("PlayEx: ParticleSystem is unassigned.");
#endif
            return;
        }

        source.Play();
    }
    static public void StopEx(this ParticleSystem source, bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("StopEx: ParticleSystem is unassigned.");
#endif
            return;
        }

        source.Stop(true, stopBehavior);
    }

    static public void StopEmitting(this ParticleSystem source, bool withChildren = true)
        => source.StopEx(withChildren);

    static public void StopAndClear(this ParticleSystem source, bool withChildren = true)
        => source.StopEx(withChildren, ParticleSystemStopBehavior.StopEmittingAndClear);
}
