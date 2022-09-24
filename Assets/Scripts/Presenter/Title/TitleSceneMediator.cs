using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Collections.Generic;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] private TitleUIHandler titleUIHandler = default;
    [SerializeField] private RestartUI restartUI = default;

    private IDisposable disposable;

    private List<Button> debugBtns;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, () => GameInfo.Instance.InitData()))
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
        var logoObservable = titleUIHandler.Logo();

        IObservable<Unit> titleObservable;

        if (DataStoreAgent.Instance.ImportGameData())
        {
            // Starts with load data.
            Debug.Log("Data load");

            var dataLoadObservable = logoObservable
                .ContinueWith(_ => Observable.Merge(sceneLoader.LoadSceneAsync(1, 0.99f), restartUI.Play()))
                .Select(_ =>
                {
                    restartUI.ActivateButtons(); // Don't allow to click buttons until the loading has finished.
                    return Unit.Default;
                })
                .Publish();

            var restartDisposable = dataLoadObservable
                .ContinueWith(_ => restartUI.Restart)
                .ContinueWith(_ => titleUIHandler.FadeOutObservable())
                .IgnoreElements()
                .Subscribe(null, () => SceneTransition(3, () => GameInfo.Instance.InitData(false)))
                .AddTo(this);

            titleObservable = dataLoadObservable
                .ContinueWith(_ => restartUI.Title)
                .Select(tween =>
                {
                    restartDisposable.Dispose();
                    return Unit.Default;
                });

            dataLoadObservable.Connect();
        }
        else
        {
            titleObservable = logoObservable.ContinueWith(_ => sceneLoader.LoadSceneAsync(1, 3f));
        }

        disposable = titleObservable
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
            GameInfo.Instance.InitData();
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
