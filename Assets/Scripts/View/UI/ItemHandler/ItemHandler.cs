using System;
using UniRx;

public class ItemHandler : BaseHandler
{
    public IObservable<Unit> ObserveGo => ObserveUp;
    public IObservable<Unit> ObserveGet => ObserveDown;
    public IObservable<Unit> ObserveInspect => ObserveRL;
    public IObservable<bool> ObserveHandOn => Observable.Merge(getItemFlickR.IsHandOn, getItemFlickL.IsHandOn);

    private GetItemFlick getItemFlickR;
    private GetItemFlick getItemFlickL;

    protected override void Awake()
    {
        base.Awake();
        getItemFlickR = flickR as GetItemFlick;
        getItemFlickL = flickL as GetItemFlick;
    }
}
