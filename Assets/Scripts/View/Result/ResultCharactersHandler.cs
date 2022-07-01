using UnityEngine;
using UniRx;
using System;

public class ResultCharactersHandler
{
    private UnityChanResultReactor unityChanReactor;
    private ResultSpotLight spotLight;
    private BagControl yenBag;
    private int numOfCoins;

    public Action StartAction { get; private set; }

    public ResultCharactersHandler(UnityChanResultReactor reactor, ResultSpotLight spotLight, ulong wagesAmount)
    {
        this.unityChanReactor = reactor;
        this.spotLight = spotLight;

        numOfCoins = (int)(wagesAmount / 500);

        if (numOfCoins > 20000)
        {
            yenBag = GameObject.Instantiate(Resources.Load<BagControl>("Prefabs/Result/BagControls"));
            StartAction = StartPress;
        }
        else
        {
            yenBag = GameObject.Instantiate(Resources.Load<BagControl>("Prefabs/Result/SmallBagControls"));
            StartAction = StartCatch;
        }
    }

    private void StartPress()
    {
        yenBag.SetPressTarget(unityChanReactor.GetPressTarget());
        spotLight.SetRange(30f);
        DropBag();
    }

    private void StartCatch()
    {
        unityChanReactor.StartCatch(yenBag.bagSize);
        DropBag();
    }

    private void DropBag()
    {
        spotLight.SetTrackTarget(yenBag.transform);
        Observable.Timer(TimeSpan.FromSeconds(0.625f)).Subscribe(_ => yenBag.Drop()).AddTo(yenBag);
    }
}
