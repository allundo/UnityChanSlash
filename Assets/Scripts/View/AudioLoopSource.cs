using UnityEngine;
using DG.Tweening;

public class AudioLoopSource : MonoBehaviour
{
    private AudioSource introSource;
    private AudioSource loopSource;
    private Tween fadeTween;

    private AudioReverbFilter reverb;
    private float distanceLevel;
    private void SetDistance(float level)
    {
        distanceLevel = level;
        volume = 1f - level;
        reverb.dryLevel = (1f - level) * 5000f;
        reverb.roomHF = (1f - level) * 10000f;
        reverb.reverbLevel = (level - 1f) * 10000f;
    }

    public float volume
    {
        get { return loopSource.volume; }
        set { introSource.volume = loopSource.volume = value; }
    }

    public bool isPlaying => introSource.isPlaying || loopSource.isPlaying;
    public bool isPausing => introSource.IsPausing() || loopSource.IsPausing();

    void Awake()
    {
        introSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();

        introSource.spatialize = loopSource.spatialize = false;
        introSource.spatialBlend = loopSource.spatialBlend = 0f;
        introSource.playOnAwake = loopSource.playOnAwake = false;
        introSource.bypassEffects = loopSource.bypassEffects = true;
        introSource.bypassListenerEffects = loopSource.bypassListenerEffects = true;
        introSource.bypassReverbZones = loopSource.bypassReverbZones = true;

        introSource.loop = false;
        loopSource.loop = true;
    }

    public void LoadClips(BGMType type, bool addReverb = false)
    {
        introSource.clip = Resources.Load<AudioClip>($"AudioClip/{type.ToString()}_intro");
        loopSource.clip = Resources.Load<AudioClip>($"AudioClip/{type.ToString()}_loop");

        if (addReverb)
        {
            reverb = gameObject.AddComponent<AudioReverbFilter>();
            SetDistance(0f);
        }

        if (type != BGMType.Dummy && introSource.clip == null || loopSource.clip == null)
        {
            Debug.LogError($"BGMtype: {type.ToString()} not found. Load dummy clip instead.");
            LoadClips(BGMType.Dummy);
        }
    }

    public AudioLoopSource Play(float volume = 1f)
    {
        fadeTween?.Kill();
        this.volume = volume;
        PlayAudioLoop();
        return this;
    }

    private void PlayAudioLoop()
    {
        var startTime = AudioSettings.dspTime;
        introSource.PlayScheduled(startTime);
        loopSource.PlayScheduled(startTime + introSource.clip.length - introSource.time);
    }

    public Tween FadeOut(float duration = 1f, bool stopOnComplete = false, Ease ease = Ease.Linear)
        => FadeTo(0f, duration).SetEase(ease).OnComplete(stopOnComplete ? Stop : Pause).Play();

    public Tween FadeIn(float duration = 1f, Ease ease = Ease.Linear)
    {
        if (!isPlaying)
        {
            if (isPausing)
            {
                UnPause();
            }
            else
            {
                Play(0f);
            }
        }

        return FadeTo(1f, duration).SetEase(ease).Play();
    }

    public Tween FadeTo(float to, float duration = 1f)
    {
        fadeTween?.Kill();
        fadeTween = DOTween.To(() => volume, value => volume = value, to, duration).SetUpdate(true);
        return fadeTween;
    }

    public Tween FadeDistance(float level, float duration = 1f, float delay = 0f)
    {
        fadeTween?.Kill();
        fadeTween = DOTween.Sequence()
            .AppendInterval(delay)
            .Join(DOTween.To(() => distanceLevel, value => SetDistance(value), level, duration).SetEase(Ease.Linear));

        if (level > 0f)
        {
            fadeTween.OnPlay(() => introSource.bypassEffects = loopSource.bypassEffects = false);
        }
        else
        {
            fadeTween.OnComplete(() => introSource.bypassEffects = loopSource.bypassEffects = true);
        }

        return fadeTween.Play();
    }

    public void Stop()
    {
        introSource.Stop();
        loopSource.Stop();
    }

    public void Pause()
    {
        if (fadeTween != null && fadeTween.IsActive()) fadeTween?.Pause();
        introSource.Pause();
        loopSource.Pause();
    }

    public void UnPause()
    {
        if (fadeTween != null && fadeTween.IsActive() && !fadeTween.IsPlaying()) fadeTween?.Play();

        if (introSource.IsPausing())
        {
            loopSource.Stop();
            PlayAudioLoop();
        }
        else if (loopSource.IsPausing())
        {
            loopSource.UnPause();
        }
    }

    public void DestroyByHandler()
    {
        fadeTween?.Kill();
        Stop();
        Resources.UnloadAsset(introSource.clip);
        Resources.UnloadAsset(loopSource.clip);
        Destroy(gameObject);
    }
}
