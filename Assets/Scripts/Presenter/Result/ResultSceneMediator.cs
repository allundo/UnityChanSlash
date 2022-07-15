using UnityEngine;
using UniRx;
using DG.Tweening;

public class ResultSceneMediator : SceneMediator
{
    [SerializeField] ResultUIHandler resultUIHandler = default;
    [SerializeField] UnityChanResultReactor unityChanReactor = default;
    [SerializeField] Transform mainCameraTf = default;
    [SerializeField] ResultSpotLight spotLight = default;

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
        var yenBag = new BagControl(resultBonus.wagesAmount);

        var charactersHandler = new ResultCharactersHandler(unityChanReactor, spotLight, yenBag);

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
            .Subscribe(_ =>
            {
                resultUIHandler.CenterResults(3f).Play();
                if (yenBag.bagSize == BagSize.Gigantic)
                {
                    mainCameraTf.DOMove(mainCameraTf.forward * -3f + Vector3.up * 2f, 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    mainCameraTf.DORotate(new Vector3(18f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                }
            })
            .AddTo(this);

    }

    private void DebugResult()
    {
        Debug.Log("DEBUG MODE");

        GameInfo gameInfo = GameInfo.Instance;
        gameInfo.clearTimeSec = 0;

        var resultBonus = new ResultBonus(gameInfo);
        var yenBag = new BagControl(resultBonus.wagesAmount);

        var charactersHandler = new ResultCharactersHandler(unityChanReactor, spotLight, yenBag);

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
            .Subscribe(_ =>
            {
                resultUIHandler.CenterResults(3f).Play();
                if (yenBag.bagSize == BagSize.Gigantic)
                {
                    mainCameraTf.DOMove(mainCameraTf.forward * -3f + Vector3.up * 2f, 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    mainCameraTf.DORotate(new Vector3(18f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                }
            })
            .AddTo(this);
    }
}
