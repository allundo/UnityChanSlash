using System;
using UniRx;

public class EventFixedOpenDoorState : DoorState, IEventHandleState
{
    public EventFixedOpenDoorState(ItemType type = ItemType.Null) : base(type) { }

    public bool isEventOn { get; protected set; } = false;
    public override bool IsControllable => !isEventOn && base.IsControllable;

    public void ForceEventOn()
    {
        if (!IsOpen) state.Value = StateEnum.FORCE_OPEN;
        isEventOn = true;
    }

    public void EventOn()
    {
        if (isEventOn) return;
        if (!IsOpen) state.Value = StateEnum.OPENING;
        isEventOn = true;
    }

    public void EventOff()
    {
        isEventOn = false;
    }
}

public class EventSealedCloseDoorState : DoorState, IEventObservableState
{
    public EventSealedCloseDoorState(ItemType type = ItemType.Null) : base(type) { }

    public bool isEventOn
    {
        get { return IsEventOn.Value; }
        protected set { IsEventOn.Value = value; }
    }

    protected IReactiveProperty<bool> IsEventOn = new ReactiveProperty<bool>(false);
    public IObservable<bool> EventObservable => IsEventOn.SkipLatestValueOnSubscribe();

    public override bool IsLocked => isEventOn || base.IsLocked;

    protected override ActiveMessageData lockedMessage
        => isEventOn ? new ActiveMessageData("扉が瘴気で封印されている！", SDFaceID.SURPRISE, SDEmotionID.EXSURPRISE) : base.lockedMessage;

    public void ForceEventOn()
    {
        if (!isBroken && IsOpen) throw new NotImplementedException("ForceEventOn() works only when importing map data");

        isEventOn = true;
    }

    public void EventOn()
    {
        if (isEventOn) return;
        isEventOn = true;
        if (!isBroken && IsOpen) state.Value = StateEnum.CLOSING;
    }

    public void EventOff()
    {
        if (!isEventOn) return;
        isEventOn = false;
    }
}
