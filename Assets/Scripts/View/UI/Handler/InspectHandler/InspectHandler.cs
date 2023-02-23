using System;
using UniRx;

public class InspectHandler : BaseHandler
{
    protected ITile tile;
    public IObservable<Unit> ObserveGo => ObserveUp;
    public IObservable<ITile> ObserveInspect => ObserveRL.Select(_ => tile);

    public void Activate(ITile tile)
    {
        this.tile = tile;
        base.Activate();
    }
}
