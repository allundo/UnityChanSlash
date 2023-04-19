using UnityEngine;
using DG.Tweening;

public class AudioLoopSource : MonoBehaviour
{
    [SerializeField] private AudioClip introClip = default;
    [SerializeField] private AudioClip loopClip = default;

    private AudioSource introSource;
    private AudioSource loopSource;

    public float volume
    {
        get { return loopSource.volume; }
        set { introSource.volume = loopSource.volume = value; }
    }

    public bool isPlaying => introSource.isPlaying || loopSource.isPlaying;
    public bool isPausing => (introSource.time > 0f && introSource.time < introClip.length) || (loopSource.time > 0f && loopSource.time < loopClip.length);

    void Awake()
    {
        introSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();

        introSource.clip = introClip;
        introSource.loop = false;

        loopSource.clip = loopClip;
        loopSource.loop = true;
    }

    public void Play()
    {
        var startTime = AudioSettings.dspTime;
        introSource.PlayScheduled(startTime);
        loopSource.PlayScheduled(startTime + introSource.clip.length);
    }

    public void FadeOut(float duration = 1f, Ease ease = Ease.OutQuad)
        => FadeTo(0f, duration).SetEase(ease).OnComplete(Stop).Play();

    public void FadeIn(float duration = 1f, Ease ease = Ease.InQuad)
    {
        if (!isPlaying)
        {
            if (isPausing)
            {
                UnPause();
            }
            else
            {
                Play();
            }
        }

        volume = 0f;
        FadeTo(1f, duration).SetEase(ease).Play();
    }

    public Tween FadeTo(float to, float duration = 1f)
    {
        return DOTween.To(() => volume, value => volume = value, to, duration);
    }

    public void Stop()
    {
        introSource.Stop();
        loopSource.Stop();
    }

    public void Pause()
    {
        introSource.Pause();
        loopSource.Pause();
    }

    public void UnPause()
    {
        introSource.UnPause();
        loopSource.UnPause();
    }
}
