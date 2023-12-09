using UnityEngine;
using UniRx;
using DG.Tweening;

public class ResultSceneMediator : SceneMediator
{
    [SerializeField] ResultUIHandler resultUIHandler = default;
    [SerializeField] UnityChanResultReactor unityChanReactor = default;
    [SerializeField] Transform mainCameraTf = default;
    [SerializeField] ResultSpotLight spotLight = default;
    [SerializeField] Transform dirLightTf = default;
    [SerializeField] GroundCoinGenerator generator = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(Result, DebugResult);

        sceneLoader.StartLoadScene(0);

        Time.timeScale = 1f;

        resultUIHandler.FadeOutScreen
            .Subscribe(_ => GroundCoin.Release())
            .AddTo(this);

        GameInfo gameInfo = GameInfo.Instance;

        resultUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0, () => gameInfo.InitData()))
            .AddTo(this);

#if UNITY_EDITOR
        if (gameInfo.isScenePlayedByEditor) gameInfo.startActionID = 1;
#endif

    }

    private void Result()
    {
        var resultBonus = new ResultBonus(GameInfo.Instance);
        var bagControl = new BagControl(resultBonus.wagesAmount, generator);

        var charactersHandler = new ResultCharactersHandler(unityChanReactor, spotLight, bagControl);

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
                resultUIHandler.CenterResults(GameInfo.Instance.clearRank, GameInfo.Instance.clearRecord, 3f).Play();
                if (bagControl.bagSize == BagSize.Gigantic)
                {
                    resultUIHandler.ClickToEnd
                        .Subscribe(_ => bagControl.StopCoinShower())
                        .AddTo(gameObject);

                    mainCameraTf.DOMove(mainCameraTf.forward * -3f + Vector3.up * 2f, 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    mainCameraTf.DORotate(new Vector3(18f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    dirLightTf.DORotate(new Vector3(-30f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    spotLight.SetAngle(20, 30f, Ease.OutCubic);
                    spotLight.SetRange(20, 30f, Ease.OutCubic);
                }
            })
            .AddTo(this);
    }

    private void DebugResult()
    {
        Debug.Log("DEBUG MODE");

        var counter = new PlayerCounter();
        for (int i = 0; i < 20; i++)
        {
            counter.IncDefeat(EnemyType.Slime);
            counter.IncAttack(EquipmentCategory.Knuckle);
        }

        GameInfo gameInfo = GameInfo.Instance;

        gameInfo.TotalClearCounts(counter, 100, 171717);
        gameInfo.clearRank = 1;
        gameInfo.level = 10;

        var resultBonus = new ResultBonus(GameInfo.Instance);

        gameInfo.clearRecord = new DataStoreAgent.ClearRecord(gameInfo.title, resultBonus.wagesAmount, gameInfo.endTimeSec, gameInfo.defeatCount);

        Result();
    }
}
