using UnityEngine;
using UniRx;
using DG.Tweening;

public class ResultSceneMediator : SceneMediator
{
    [SerializeField] ResultUIHandler resultUIHandler = default;
    [SerializeField] UnityChanResultReactor unityChanReactor = default;
    [SerializeField] Transform mainCameraTf = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(Result, DebugResult);

        sceneLoader.StartLoadScene(0);

        Time.timeScale = 1f;

        GameInfo gameInfo = GameInfo.Instance;

        resultUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, gameInfo.ClearMaps))
            .AddTo(this);

#if UNITY_EDITOR
        if (gameInfo.isScenePlayedByEditor) gameInfo.startActionID = 1;
#endif

    }

    private void Result()
    {
        var resultBonus = new ResultBonus(GameInfo.Instance);
        var charactersHandler = new ResultCharactersHandler(unityChanReactor, resultBonus.wagesAmount);

        resultUIHandler
            .ViewResult(resultBonus)
            .Subscribe(_ =>
            {
                DOTween.Sequence()
                    .Join(resultUIHandler.SweepResults(2f))
                    .Join(mainCameraTf.DOMoveX(0f, 2f).SetEase(Ease.OutCubic))
                    .AppendCallback(() => charactersHandler.StartAction())
                    .Play();
            })
            .AddTo(this);

        unityChanReactor.ScreenOut
            .Subscribe(_ => resultUIHandler.CenterResults(3f).Play())
            .AddTo(this);

    }

    private void DebugResult()
    {
        Debug.Log("DEBUG MODE");

        GameInfo gameInfo = GameInfo.Instance;
        gameInfo.clearTimeSec = 7200;

        var resultBonus = new ResultBonus(gameInfo);
        var charactersHandler = new ResultCharactersHandler(unityChanReactor, resultBonus.wagesAmount);

        resultUIHandler
            .ViewResult(resultBonus)
            .Subscribe(_ =>
            {
                DOTween.Sequence()
                    .Join(resultUIHandler.SweepResults(2f))
                    .Join(mainCameraTf.DOMoveX(0f, 2f).SetEase(Ease.OutCubic))
                    .AppendCallback(() => charactersHandler.StartAction())
                    .Play();
            })
            .AddTo(this);

        unityChanReactor.ScreenOut
            .Subscribe(_ => resultUIHandler.CenterResults(3f).Play())
            .AddTo(this);
    }
}
