using System;
using UniRx;

public class InspectHandler : BaseHandler
{
    protected ITile tile;
    public IObservable<Unit> ObserveGo => ObserveUp;
    public IObservable<ActiveMessageData> ObserveInspect => ObserveRL.Select(_ => ActiveMessageData.InspectTile(tile));

    public void Activate(ITile tile)
    {
        this.tile = tile;
        base.Activate();
    }
}
