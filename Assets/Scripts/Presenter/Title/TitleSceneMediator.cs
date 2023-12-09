using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Collections.Generic;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] private TitleUIHandler titleUIHandler = default;
    [SerializeField] private RestartUI restartUI = default;
    [SerializeField] private AudioSource openRestartSnd = default;
    [SerializeField] private AudioSource loadStartSnd = default;

    private IDisposable disposable;

    private List<Button> debugBtns;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo, BackToTitle);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, () => GameInfo.Instance.InitData()))
            .AddTo(this);

        titleUIHandler.SettingsButtonSignal
            .Subscribe(_ => TransitOptionalScene(5))
            .AddTo(this);

        titleUIHandler.ResultsButtonSignal
            .Subscribe(_ => TransitOptionalScene(4))
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

            BagSize[] size = Util.GetValues<BagSize>();
            i = 0;

            titleUIHandler.debugResult.ForEach(btn =>
            {
                IObservable<BagSize> ret = Observable.Return(size[i++]);

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

    // Start Action ID: 0
    private void Logo()
    {
        var logoObservable = titleUIHandler.Logo();

        IObservable<Unit> titleObservable;

        if (DataStoreAgent.Instance.ImportGameData())
        {
            // Starts with load data.
            Debug.Log("Data load");

            var dataLoadObservable = logoObservable
                .ContinueWith(_ =>
                {
                    openRestartSnd.PlayEx();
                    return Observable.Merge(sceneLoader.LoadSceneAsync(1, 0.99f), restartUI.Play());
                })
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
                .Subscribe(null, () =>
                {
                    BGMManager.Instance.LoadFloor(GameInfo.Instance.currentFloor);
                    SceneTransition(3, () => GameInfo.Instance.InitData(false)); // GameManager.LoadDataStart()
                })
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
            titleObservable = logoObservable.ContinueWith(_ =>
            {
                loadStartSnd.PlayEx();
                return sceneLoader.LoadSceneAsync(1, 3f);
            });
        }

        disposable = titleObservable
            .IgnoreElements()
            .Subscribe(null, () =>
            {
                BGMManager.Instance.PlayTitle();
                titleUIHandler.ToTitle();
            })
            .AddTo(this);
    }

    // Start Action ID: 1
    private void SkipLogo()
    {
        disposable = sceneLoader.LoadSceneAsync(1)
            .IgnoreElements()
            .Subscribe(null, () =>
            {
                BGMManager.Instance.PlayTitle();
                titleUIHandler.SkipLogo();
            })
            .AddTo(this);
    }

    // Start Action ID: 2
    private void BackToTitle()
    {
        disposable = sceneLoader.LoadSceneAsync(1)
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.SkipLogo)
            .AddTo(this);
    }

    private void TransitOptionalScene(int sceneBuildIndex)
    {
        DisableOtherControls();
        ForceTransitScene(sceneBuildIndex, 0);
    }

    private void DisableOtherControls()
    {
        disposable.Dispose();
        sceneLoader.UnloadCurrentLoadingScene();

        if (Debug.isDebugBuild) DisableDebugBtns();
    }

    // ## FOR DEBUG (begin)

    private void DebugStart(int floor)
    {
        DisableOtherControls();
        BGMManager.Instance.Stop();

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
        BGMManager.Instance.Stop();

        GameInfo.Instance.endTimeSec = clearTimeSec;

        ForceTransitScene(2, 0);
    }

    private void DebugResult(BagSize size)
    {
        DisableOtherControls();
        BGMManager.Instance.Stop();

        ForceTransitScene(3, (int)size + 1);
    }

    private void DisableDebugBtns()
        => debugBtns.ForEach(btn => btn.gameObject.SetActive(false));

    // ## FOR DEBUG (end)
}
