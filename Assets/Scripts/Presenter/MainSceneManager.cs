using UnityEngine;
using UniRx;
using System;

public class MainSceneManager : BaseSceneManager
{
    [SerializeField] private GameOverWindowUI gameOver = default;
    [SerializeField] private CoverScreen cover = default;

    void Start()
    {
        gameOver.restartButton
            .OnPressedCompleteAsObservable()
            .ContinueWith(_ => LoadSceneWithCoverOn(1))
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(1))
            .AddTo(this);

    }

    private IObservable<Unit> LoadSceneWithCoverOn(int sceneBuildIndex, float duration = 2f)
        => Observable.Merge(
            sceneLoader.LoadSceneAsync(sceneBuildIndex),
            cover.CoverOn(duration).OnCompleteAsObservable().Select(t => Unit.Default)
        );
}
