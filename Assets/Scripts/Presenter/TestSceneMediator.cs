using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class TestSceneMediator : SceneMediator
{
    [SerializeField] private Button button = default;
    [SerializeField] private CoverScreen cover = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(() => { });

        button.onClick
            .AsObservable()
            .First(_ => true)
            .ContinueWith(_ => LoadSceneWithCoverOn(2))
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(0))
            .AddTo(this);
    }

    private IObservable<Unit> LoadSceneWithCoverOn(int sceneBuildIndex, float duration = 2f)
        => cover.CoverOnObservable(duration, sceneLoader.LoadSceneAsync(sceneBuildIndex));
}
