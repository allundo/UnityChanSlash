using UnityEngine;

public abstract class AnimationFX : MonoBehaviour
{
    protected FXPlayer fx;

    public virtual void Awake()
    {
        fx = new FXPlayer();
    }

    public abstract void StopVFX();

    protected class FXPlayer
    {
        public void Play(AudioSource sfx)
        {
            sfx.PlayEx();
        }

        public void PlayPitch(AudioSource sfx, float pitch)
        {
            sfx.SetPitch(pitch);

            sfx.PlayEx();
        }

        public void PlayPitch(AudioSource sfx, float minPitch = 0.7f, float maxPitch = 1.3f)
        {
            PlayPitch(sfx, Random.Range(minPitch, maxPitch));
        }

        public void Play(AudioSource sfx, ParticleSystem vfx)
        {
            sfx.PlayEx();
            vfx?.Play();
        }

        public void Play(AudioSource sfx, MoveParticle vfx)
        {
            sfx.PlayEx();
            vfx?.Play();
        }

        public void PlayPitch(AudioSource sfx, ParticleSystem vfx, float minPitch = 0.7f, float maxPitch = 1.3f)
        {
            PlayPitch(sfx, minPitch, maxPitch);
            vfx?.Play();
        }

        public void PlayPitch(AudioSource sfx, MoveParticle vfx, float minPitch = 0.7f, float maxPitch = 1.3f)
        {
            PlayPitch(sfx, minPitch, maxPitch);
            vfx?.Play();
        }

        public void Stop(AudioSource sfx)
        {
            sfx.StopEx();
        }

        public void Stop(AudioSource sfx, ParticleSystem vfx)
        {
            sfx.StopEx();
            vfx?.Stop();
        }
        public void Stop(AudioSource sfx, MoveParticle vfx)
        {
            sfx.StopEx();
            vfx?.Stop();
        }

        public void StopEmitting(ParticleSystem vfx)
        {
            vfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void StopEmitting(MoveParticle vfx)
        {
            vfx?.Stop(ParticleSystemStopBehavior.StopEmitting);
        }
    }
}