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
        SetStartActions(gm.DropStart, gm.Restart, gm.DebugStart, gm.LoadDataStart);

        BGMManager.Instance.SetWitchInfo(SpawnHandler.Instance.GetWitchInfo());

        gameOver.restartButton
            .OnPressedCompleteAsObservable()
            .ContinueWith(_ =>
            {
                BGMManager.Instance.ToRestart();
                return LoadSceneWithCoverOn(1);
            })
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(1, () => GameInfo.Instance.InitData())) // GameManager.Restart()
            .AddTo(this);

        gameOver.titleButton
            .OnPressedCompleteAsObservable()
            .ContinueWith(_ =>
            {
                BGMManager.Instance.FadeToNextScene(BGMType.Title);
                return LoadSceneWithCoverOn(0);
            })
            .IgnoreElements()
            .Subscribe(null, () => SceneTransition(1, () => GameInfo.Instance.InitData())) // TitleSceneMediator.SkipLogo()
            .AddTo(this);

        gm.ExitObservable
            .Subscribe(null, () => LoadSceneAndTransit(2, 0)) // EndingSceneMediator.StartScene()
            .AddTo(this);

#if UNITY_EDITOR
        if (GameInfo.Instance.isScenePlayedByEditor) GameInfo.Instance.startActionID = 2;
#endif
    }

    private IObservable<Unit> LoadSceneWithCoverOn(int sceneBuildIndex, float duration = 2f)
        => cover.CoverOnObservable(duration, sceneLoader.LoadSceneAsync(sceneBuildIndex));
}
