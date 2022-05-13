using System;
using UniRx;

public class ItemHandler : BaseHandler
{
    protected ItemInfo itemInfo;
    public IObservable<Unit> ObserveGo => ObserveUp;
    public IObservable<Unit> ObserveGet => ObserveDown;
    public IObservable<MessageData[]> ObserveInspect => ObserveRL.Select(_ => MessageData.ItemDescription(itemInfo));
    public IObservable<bool> ObserveHandOn => Observable.Merge(getItemFlickR.IsHandOn, getItemFlickL.IsHandOn);

    private GetItemFlick getItemFlickR;
    private GetItemFlick getItemFlickL;

    protected override void Awake()
    {
        base.Awake();
        getItemFlickR = flickR as GetItemFlick;
        getItemFlickL = flickL as GetItemFlick;
    }

    public void Activate(ItemInfo itemInfo)
    {
        this.itemInfo = itemInfo;
        base.Activate();
    }
}
