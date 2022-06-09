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
        endingUIHandler.StartScroll(SelectPeriodIndex(GameInfo.Instance.clearTimeSec))
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }

    private void DebugScene()
    {
        endingUIHandler.StartScroll()
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }

    private int SelectPeriodIndex(ulong clearTimeSec)
    {
        int period = 0;

        for (ulong elapsed = 3600; clearTimeSec > elapsed && period < 3; elapsed += 3600, period++) ;

        return period;
    }
}
