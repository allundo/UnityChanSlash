using UnityEngine;
using UniRx;
using System;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    private IDisposable logoDisposable;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, GameInfo.Instance.ClearMaps))
            .AddTo(this);

        // DEBUG ONLY
        if (Debug.isDebugBuild)
        {
            titleUIHandler.debugStart
                .OnClickAsObservable()
                .Subscribe(_ => DebugStart())
                .AddTo(this);
        }
    }

    private void Logo()
    {
        logoDisposable = titleUIHandler.Logo()
            .ContinueWith(_ => sceneLoader.LoadSceneAsync(1, 3f))
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.ToTitle)
            .AddTo(this);
    }

    private void SkipLogo()
    {
        sceneLoader.LoadSceneAsync(1)
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.SkipLogo)
            .AddTo(this);
    }

    private void DebugStart()
    {
        logoDisposable.Dispose();
        titleUIHandler.debugStart.gameObject.SetActive(false);
        sceneLoader.StartLoadScene(1);
        SceneTransition(2);
    }
}
