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
        SetStartActions(Result, SResult, MResult, LResult, XLResult);

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
        if (gameInfo.isScenePlayedByEditor) gameInfo.startActionID = 4;
#endif

    }

    private void Result()
    {
        BGMManager.Instance.PlaySceneBGM(BGMType.Result);

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

        if (bagControl.bagSize == BagSize.Gigantic)
        {
            unityChanReactor.ScreenOut
                .Subscribe(_ =>
                {
                    resultUIHandler.CenterResults().Play();

                    resultUIHandler.ClickToEnd
                        .Subscribe(_ => bagControl.StopCoinShower())
                        .AddTo(gameObject);

                    mainCameraTf.DOMove(mainCameraTf.forward * -3f + Vector3.up * 2f, 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    mainCameraTf.DORotate(new Vector3(18f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    dirLightTf.DORotate(new Vector3(-30f, 0, 0), 30f).SetRelative().SetEase(Ease.OutCubic).Play();
                    spotLight.SetAngle(20, 30f, Ease.OutCubic);
                    spotLight.SetRange(20, 30f, Ease.OutCubic);

                })
                .AddTo(this);
        }
        else
        {
            unityChanReactor.ScreenOut
                .Subscribe(_ => resultUIHandler.CenterResults().Play(), bagControl.DisableCloth)
                .AddTo(this);
        }
    }

    private void SResult() => DebugResult();
    private void MResult() => DebugResult();
    private void LResult() => DebugResult();
    private void XLResult() => DebugResult();

    private void DebugResult()
    {
        Debug.Log("DEBUG MODE");

        GameInfo gameInfo = GameInfo.Instance;
        BagSize size = Util.ConvertTo<BagSize>((int)gameInfo.startActionID - 1);

        var counter = new PlayerCounter();

        int clearTimeSec = 10;
        int level = 1;
        int strength = 1000;
        int magic = 1000;
        ulong moneyAmount = 4096;

        for (int i = 0; i < 3 - (int)size; ++i)
        {
            for (int j = 0; j < 20; j++)
            {
                counter.IncStep();
                clearTimeSec += 80;
                strength -= 12;
                magic -= 12;
            }
        }

        for (int i = 0; i < (int)size; ++i)
        {
            for (int j = 0; j < 4; j++)
            {
                counter.IncDefeat(EnemyType.Slime);
                counter.IncDefeat(EnemyType.SkeletonSoldier);
                counter.IncAttack(EquipmentCategory.Knuckle);
                moneyAmount *= 2;
                level += 3;
            }
        }

        gameInfo.TotalClearCounts(counter, clearTimeSec, moneyAmount);
        gameInfo.level = level;
        gameInfo.strength = strength;
        gameInfo.magic = magic;
        gameInfo.clearRank = 4 - (int)size;

        var resultBonus = new ResultBonus(gameInfo);
        gameInfo.clearRecord = new DataStoreAgent.ClearRecord(gameInfo.title, resultBonus.wagesAmount, gameInfo.endTimeSec, gameInfo.defeatCount);

        Result();
    }
}
