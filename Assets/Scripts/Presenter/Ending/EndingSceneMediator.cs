using UnityEngine;
using UniRx;

public class EndingSceneMediator : SceneMediator
{
    [SerializeField] public EndingUIHandler endingUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(StartScene, DebugScene);

        sceneLoader.StartLoadScene(3);

        Time.timeScale = 1f;

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor) GameInfo.Instance.startActionID = 1;
#endif

    }

    private void StartScene()
    {
        BGMManager.Instance.PlaySceneBGM(BGMType.End);

        endingUIHandler.StartScroll(SelectPeriodIndex(GameInfo.Instance.endTimeSec))
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }

    private void DebugScene()
    {
        BGMManager.Instance.PlaySceneBGM(BGMType.End);

        endingUIHandler.StartScroll()
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }

    private int SelectPeriodIndex(int clearTimeSec)
    {
        int period = 0;

        for (int elapsed = 1800; clearTimeSec > elapsed && period < 3; elapsed += 1800, period++) ;

        return period;
    }
}
