using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BGMManager : SingletonMonoBehaviour<BGMManager>
{
    private Dictionary<BGMType, AudioLoopSource> BGMs = new Dictionary<BGMType, AudioLoopSource>();
    private readonly BGMType[] FLOOR_BGM_TYPE = new BGMType[] { BGMType.TwistedHeart, BGMType.TwistedHeart, BGMType.Fire, BGMType.Ice, BGMType.Ruin };
    private AudioLoopSource floorBGM;
    private AudioLoopSource currentBGM;
    private Tween reserveTween;

    public void Load(BGMType type)
    {
        currentBGM = SelectSource(type);
    }

    public void SwitchFloor(int floor, float duration = 1f, bool stopOnComplete = false)
    {
        floorBGM = SelectSource(FLOOR_BGM_TYPE[floor - 1]);
        if (currentBGM != floorBGM && currentBGM != null) FadeOut(duration, stopOnComplete);
    }

    public void PlayFloorBGM()
    {
        if (currentBGM != floorBGM)
        {
            currentBGM = floorBGM;
            if (currentBGM.isPausing) currentBGM.FadeIn(0.5f, Ease.OutQuad);
            if (!currentBGM.isPlaying) currentBGM.Play();
        }
    }

    public void SwitchBossBGM()
    {
        if (currentBGM == SelectSource(BGMType.GameOver)) return;

        reserveTween?.Kill();
        floorBGM.FadeOut(1f);
        currentBGM = SelectSource(BGMType.Witch);
        currentBGM.FadeIn(0.25f);
    }

    public void ReserveBackToFloorBGM()
    {
        if (currentBGM != SelectSource(BGMType.Witch)) return;

        currentBGM.FadeOut(10f);
        reserveTween = DOVirtual.DelayedCall(10f, () =>
        {
            currentBGM = floorBGM;
            currentBGM.FadeIn(1f);
        }, false).Play();
    }

    public void Play(BGMType type, bool stopCurrent = true, bool restartSameSrc = false)
    {
        var src = SelectSource(type);
        if (!restartSameSrc && currentBGM == src)
        {
            if (currentBGM.isPausing) currentBGM.UnPause();
            if (!currentBGM.isPlaying) currentBGM.Play();
            return;
        }

        reserveTween?.Kill();
        if (stopCurrent && currentBGM != null && currentBGM.isPlaying) currentBGM.Stop();
        currentBGM = src.Play();
    }

    public void Stop() => currentBGM?.Stop();

    public Tween FadeOut(float duration = 1f, bool stopOnComplete = false, Ease ease = Ease.Linear)
    {
        reserveTween?.Kill();
        if (currentBGM != null && currentBGM.isPlaying) return currentBGM.FadeOut(duration, stopOnComplete, ease);
        return null;
    }

    public void CrossFade(BGMType type, float outDuration, float inDuration, float interval = 0f, bool stopOnComplete = false)
    {
        FadeOut(outDuration, stopOnComplete);
        reserveTween = DOVirtual.DelayedCall(Mathf.Max(0, outDuration + interval), () =>
        {
            currentBGM = SelectSource(type);
            currentBGM.FadeIn(inDuration);
        }, false).Play();
    }

    public void SetDistance(float level, float duration = 1f, float delay = 0f)
        => currentBGM?.FadeDistance(level, duration, delay);

    private AudioLoopSource SelectSource(BGMType type) => BGMs.LazyLoad(type, type => LoadSource(type));
    private AudioLoopSource LoadSource(BGMType type)
    {
        var audio = new GameObject(type.ToString());
        var src = audio.AddComponent<AudioLoopSource>();
        src.LoadClips(type, type == BGMType.Title);
        audio.transform.SetParent(transform);
        return src;
    }

    public void ReleaseAllBGMs(BGMType exceptFor = BGMType.Title)
    {
        Util.GetValues<BGMType>().ForEach(type =>
        {
            AudioLoopSource src;
            if (BGMs.TryGetValue(type, out src))
            {
                src.DestroyByHandler();
                BGMs.Remove(type);
            }
        }, exceptFor);
    }
}
