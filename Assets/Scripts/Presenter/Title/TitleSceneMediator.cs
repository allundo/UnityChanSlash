using UnityEngine;
using UniRx;
using System;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    private IDisposable disposable;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, GameInfo.Instance.ClearMaps))
            .AddTo(this);

        // DEBUG ONLY
        if (Debug.isDebugBuild)
        {
            int count = 0;
            titleUIHandler.debugStart.ForEach(btn =>
            {
                IObservable<int> ret = Observable.Return(++count);

                btn.OnClickAsObservable()
                    .First()
                    .ContinueWith(_ => ret)
                    .Subscribe(floor => DebugStart(floor))
                    .AddTo(this);
            });
        }
    }

    private void Logo()
    {
        disposable = titleUIHandler.Logo()
            .ContinueWith(_ => sceneLoader.LoadSceneAsync(1, 3f))
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.ToTitle)
            .AddTo(this);
    }

    private void SkipLogo()
    {
        disposable = sceneLoader.LoadSceneAsync(1)
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.SkipLogo)
            .AddTo(this);
    }

    private void DebugStart(int floor)
    {
        GameInfo.Instance.CreateDebugMap();
        GameInfo.Instance.currentFloor = floor;

        disposable.Dispose();
        titleUIHandler.debugStart.ForEach(btn => btn.gameObject.SetActive(false));
        sceneLoader.StartLoadScene(1);
        SceneTransition(2);
    }
}
