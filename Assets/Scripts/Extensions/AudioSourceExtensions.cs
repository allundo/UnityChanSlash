using UnityEngine;

static public class AudioSourceExtensions
{
    static public void PlayEx(this AudioSource source, float delay = 0)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("PlayEx: AudioSource is unassigned.");
#endif
            return;
        }

        source.PlayDelayed(delay);
    }
    static public void StopEx(this AudioSource source)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("StopEx: AudioSource is unassigned.");
#endif
            return;
        }

        source.Stop();
    }

    static public void SetPitch(this AudioSource source, float pitch = 1f)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("SetPitch: AudioSource is unassigned.");
#endif
            return;
        }

        source.pitch = pitch;
    }
}
