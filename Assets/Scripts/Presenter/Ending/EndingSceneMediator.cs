using UnityEngine;
using UniRx;
using DG.Tweening;

public class EndingSceneMediator : SceneMediator
{
    [SerializeField] EndingUIHandler endingUIHandler = default;

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
        endingUIHandler.StartScroll()
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }

    private void DebugScene()
    {
        endingUIHandler.StartScroll()
           .Subscribe(_ => SceneTransition(0))
           .AddTo(this);
    }
}
