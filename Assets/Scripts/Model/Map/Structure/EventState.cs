using System;
using UniRx;

public interface IEventState
{
    void EventOn();
}

public interface IEventHandleState : IEventState
{
    bool isEventOn { get; }
    void ForceEventOn();
    void EventOff();
}

public class FountainState : IEventHandleState
{
    public bool isEventOn
    {
        get { return IsEventOn.Value; }
        protected set { IsEventOn.Value = value; }
    }

    protected IReactiveProperty<bool> IsEventOn = new ReactiveProperty<bool>(false);
    public IObservable<bool> EventObservable => IsEventOn.SkipLatestValueOnSubscribe();

    public void ForceEventOn() => EventOn();

    public void EventOn()
    {
        isEventOn = true;
    }

    public void EventOff()
    {
        isEventOn = false;
    }
}
