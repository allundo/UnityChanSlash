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
    private IWitchInfo witchInfo;

    private bool isWitchBGMContinued => ItemInventory.Instance.hasKeyBlade() && currentBGM == SelectSource(BGMType.Witch);

    public void SetWitchInfo(IWitchInfo info)
    {
        witchInfo = info;
    }

    public void Load(BGMType type)
    {
        currentBGM = SelectSource(type);
    }

    public void ToRestart()
    {
        var type = FLOOR_BGM_TYPE[0];
        floorBGM = SelectSource(type);
        FadeToNextScene(type);
    }

    public void FadeToNextScene(BGMType type, float duration = 2f, bool loadSource = false)
    {
        if (loadSource) LoadSource(type);

        FadeOut(duration, true);
        reserveTween = DOVirtual.DelayedCall(2f, () => ReleaseAllBGMs(type)).Play();
    }

    public void GameOver()
    {
        FadeOut(2f, true);
        reserveTween = DOVirtual.DelayedCall(3f, () => currentBGM = SelectSource(BGMType.GameOver).Play(), false).Play();
    }

    public void LoadFloor(int floor)
    {
        floorBGM = SelectSource(FLOOR_BGM_TYPE[floor - 1]);
    }

    public void SwitchFloor(int floor, float duration = 1f, bool stopOnComplete = false)
    {
        LoadFloor(floor);
        if (currentBGM != floorBGM && currentBGM != null && !isWitchBGMContinued) FadeOut(duration, stopOnComplete);
    }

    public void PlayFloorBGM()
    {
        if (witchInfo.IsWitchLiving || ItemInventory.Instance.hasKeyBlade())
        {
            SwitchBossBGM();
            return;
        }

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

        var bossBGM = SelectSource(BGMType.Witch);

        if (currentBGM != bossBGM)
        {
            reserveTween?.Kill();
            floorBGM.FadeOut(1f);
            currentBGM = bossBGM;
        }

        currentBGM.FadeIn(0.75f);
    }

    public void ReserveBackToFloorBGM()
    {
        if (currentBGM != SelectSource(BGMType.Witch)) return;

        currentBGM.FadeOut(10f);
        currentBGM = floorBGM;
        reserveTween = DOVirtual.DelayedCall(9f, () => currentBGM.FadeIn(1f), false).Play();
    }

    public void PlaySceneBGM(BGMType type)
    {
        reserveTween?.Kill();
        if (currentBGM != null && currentBGM.isPlaying) currentBGM.Stop();
        currentBGM = SelectSource(type).Play();
    }

    public void Stop() => currentBGM?.Stop();

    public Tween FadeOut(float duration = 1f, bool stopOnComplete = false, Ease ease = Ease.Linear)
    {
        reserveTween?.Kill();
        if (currentBGM != null && currentBGM.isPlaying) return currentBGM.FadeOut(duration, stopOnComplete, ease);
        return null;
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

    private void ReleaseAllBGMs(BGMType exceptFor = BGMType.Title)
    {
        Util.GetValues<BGMType>().ForEach(type => ReleaseBGM(type), exceptFor);
    }

    private void ReleaseBGM(BGMType type)
    {
        AudioLoopSource src;
        if (BGMs.TryGetValue(type, out src))
        {
            src.DestroyByHandler();
            BGMs.Remove(type);
        }
    }

    private void OnDestroy()
    {
        reserveTween?.Kill();
        ReleaseAllBGMs();
        ReleaseBGM(BGMType.Title);
    }
}
