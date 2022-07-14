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

    public ResultCharactersHandler(UnityChanResultReactor reactor, ResultSpotLight spotLight, BagControl yenBag)
    {
        this.unityChanReactor = reactor;
        this.spotLight = spotLight;

        this.yenBag = yenBag;

        if (yenBag.bagSize == BagSize.Gigantic)
        {
            StartAction = StartPress;
        }
        else
        {
            StartAction = StartCatch;
        }
    }

    private void StartPress()
    {
        yenBag.SetPressTarget(unityChanReactor.GetPressTarget());
        spotLight.SetAngle(15f);
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
