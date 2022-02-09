using UnityEngine;
public class AnimationFX : MonoBehaviour
{
    protected void Play(AudioSource sfx)
    {
        sfx.PlayEx();
    }

    protected void Play(AudioSource sfx, float minPitch = 0.7f, float maxPitch = 1.3f)
    {
        sfx.SetPitch(Random.Range(minPitch, maxPitch));

        sfx.PlayEx();
    }

    protected void Play(AudioSource sfx, ParticleSystem vfx)
    {
        sfx.PlayEx();
        vfx?.Play();
    }

    protected void Play(AudioSource sfx, ParticleSystem vfx, float minPitch = 0.7f, float maxPitch = 1.3f)
    {
        Play(sfx, minPitch, maxPitch);
        vfx?.Play();
    }

    protected void Stop(AudioSource sfx)
    {
        sfx.StopEx();
    }

    protected void Stop(AudioSource sfx, ParticleSystem vfx)
    {
        sfx.StopEx();
        vfx?.Stop();
    }
}