using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Collections.Generic;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    private IDisposable disposable;

    private List<Button> debugBtns;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, GameInfo.Instance.ClearMaps))
            .AddTo(this);

        titleUIHandler.ResultsButtonSignal
            .Subscribe(_ => ForceTransitScene(4, 0))
            .AddTo(this);

        // ## FOR DEBUG (begin)
        // DEBUG ONLY
        if (Debug.isDebugBuild)
        {
            debugBtns = new List<Button>();

            int count = 0;

            titleUIHandler.debugStart.ForEach(btn =>
            {
                // Reactive extension cannot bound loop index variable,
                // so need to decide return value here.
                IObservable<int> ret = Observable.Return(++count);

                btn.OnClickAsObservable()
                    .First()
                    .ContinueWith(_ => ret)
                    .Subscribe(floor => DebugStart(floor))
                    .AddTo(this);

                debugBtns.Add(btn);
            });

            int[] clearTimes = new int[] { 2000, 6000, 8000, 80000 };
            int i = 0;

            titleUIHandler.debugEnding.ForEach(btn =>
            {
                IObservable<int> ret = Observable.Return(clearTimes[i++]);

                btn.OnClickAsObservable()
                    .First()
                    .ContinueWith(_ => ret)
                    .Subscribe(time => DebugEnding(time))
                    .AddTo(this);

                debugBtns.Add(btn);
            });

            clearTimes = new int[] { 7200, 3500, 3200, 1000 };
            i = 0;

            titleUIHandler.debugResult.ForEach(btn =>
            {
                IObservable<int> ret = Observable.Return(clearTimes[i++]);

                btn.OnClickAsObservable()
                    .First()
                    .ContinueWith(_ => ret)
                    .Subscribe(time => DebugResult(time))
                    .AddTo(this);

                debugBtns.Add(btn);
            });
        }
        // ## FOR DEBUG (end)

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

    // ## FOR DEBUG (begin)

    private void DebugStart(int floor)
    {
        DisableOtherControls();

        if (floor == 2)
        {
            GameInfo.Instance.CreateDebugMap();
        }
        else
        {
            GameInfo.Instance.ClearMaps();
        }

        GameInfo.Instance.currentFloor = floor;

        ForceTransitScene(1, 2);
    }

    private void DebugEnding(int clearTimeSec)
    {
        DisableOtherControls();

        GameInfo.Instance.clearTimeSec = clearTimeSec;

        ForceTransitScene(2, 0);
    }

    private void DebugResult(int clearTimeSec)
    {
        DisableOtherControls();

        GameInfo.Instance.clearTimeSec = clearTimeSec;

        ForceTransitScene(3, 0);
    }

    private void DisableOtherControls()
    {
        disposable.Dispose();
        sceneLoader.UnloadCurrentLoadingScene();
        debugBtns.ForEach(btn => btn.gameObject.SetActive(false));
    }

    // ## FOR DEBUG (end)
}
