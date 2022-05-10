using UnityEngine;
using UniRx;
using DG.Tweening;

public class ResultSceneMediator : SceneMediator
{
    [SerializeField] ResultUIHandler resultUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(Result, DebugResult);

        sceneLoader.StartLoadScene(0);

        Time.timeScale = 1f;

        resultUIHandler.TransitSignal
            .Subscribe(_ =>
            {
                Debug.Log("To title");
                SceneTransition(0, GameInfo.Instance.ClearMaps);
            })
            .AddTo(this);

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor) GameInfo.Instance.startActionID = 1;
#endif

    }

    private void Result()
    {
        resultUIHandler.ViewResult().Play();
    }

    private void DebugResult()
    {
        resultUIHandler.ViewResult().Play();
    }
}
