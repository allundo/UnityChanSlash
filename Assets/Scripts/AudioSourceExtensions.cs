using UnityEngine;

static public class AudioSourceExtensions
{
    static public void PlayEx(this AudioSource source, ulong delay = 0)
    {
        if (source == null)
        {
#if UNITY_EDITOR
            Debug.Log("PlayEx: AudioSource is unassigned.");
#endif
            return;
        }

        source.Play(delay);
    }
}
