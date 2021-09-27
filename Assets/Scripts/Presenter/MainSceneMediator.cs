using UnityEngine;
using UniRx;
using System;

public class MainSceneMediator : SceneMediator
{
    [SerializeField] private GameOverWindowUI gameOver = default;
    [SerializeField] private CoverScreen cover = default;

    protected override void InitBeforeStart()
    {
        var gm = GameManager.Instance;
        SetStartActions(gm.DropStart, gm.Restart, gm.DebugStart);

        gameOver.restartButton
            .OnPressedCompleteAsObservable()
            .ContinueWith(_ => LoadSceneWithCoverOn(1))
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(1, GameInfo.Instance.ClearMaps))
            .AddTo(this);

        gameOver.titleButton
            .OnPressedCompleteAsObservable()
            .ContinueWith(_ => LoadSceneWithCoverOn(0))
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(1, GameInfo.Instance.ClearMaps))
            .AddTo(this);
    }

    private IObservable<Unit> LoadSceneWithCoverOn(int sceneBuildIndex, float duration = 2f)
        => Observable.Merge(
            sceneLoader.LoadSceneAsync(sceneBuildIndex),
            cover.CoverOn(duration).OnCompleteAsObservable().Select(t => Unit.Default)
        );
}
