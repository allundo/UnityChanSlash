using UnityEngine;
using System;
using DG.Tweening;

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
        DOTween.Sequence()
            .InsertCallback(0.625f, yenBag.Drop)
            .InsertCallback(1.2f, () => spotLight.SetTrackTarget(yenBag.transform))
            .Play();
    }
}
