using UnityEngine;
using UniRx;
using System;

public class ResultCharactersHandler
{
    private UnityChanResultReactor unityChanReactor = default;
    private BagControl yenBag;
    private int numOfCoins;

    public Action StartAction { get; private set; }

    public ResultCharactersHandler(UnityChanResultReactor reactor, ulong wagesAmount)
    {
        unityChanReactor = reactor;

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
        DropBag();
    }

    private void StartCatch()
    {
        unityChanReactor.StartCatch(yenBag.bagSize);
        DropBag();
    }

    private void DropBag()
    {
        Observable.Timer(TimeSpan.FromSeconds(0.625f)).Subscribe(_ => yenBag.Drop()).AddTo(yenBag);
    }
}
