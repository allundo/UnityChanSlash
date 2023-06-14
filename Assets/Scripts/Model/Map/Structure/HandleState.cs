using UniRx;
using System;

public interface IHandleState
{
    IReadOnlyReactiveProperty<HandleState.StateEnum> State { get; }
    IObservable<bool> LockedState { get; }

    bool IsOpen { get; }
    bool IsControllable { get; }
    bool IsLocked { get; }

    void TransitToNextState();
    void Open();
}

public abstract class HandleState : IHandleState
{
    public enum StateEnum
    {
        OPEN,
        CLOSE,
        OPENING,
        CLOSING,
        FORCE_OPEN,
        DESTRUCT,
    }

    public HandleState(ItemType type = ItemType.Null)
    {
        lockType = new ReactiveProperty<ItemType>(type);
    }

    protected IReactiveProperty<StateEnum> state = new ReactiveProperty<StateEnum>(StateEnum.CLOSE);
    public IReadOnlyReactiveProperty<StateEnum> State => state;

    public bool IsOpen => State.Value == StateEnum.OPEN || State.Value == StateEnum.OPENING;
    public abstract bool IsControllable { get; }

    protected IReactiveProperty<ItemType> lockType;
    public IObservable<bool> LockedState => lockType.SkipLatestValueOnSubscribe().Select(type => type != ItemType.Null);
    public abstract bool IsLocked { get; }

    public void TransitToNextState() => state.Value = GetNextState();
    public void Open()
    {
        state.Value = StateEnum.FORCE_OPEN;
    }

    private StateEnum GetNextState()
    {
        switch (State.Value)
        {
            case StateEnum.CLOSE:
                return IsLocked ? StateEnum.CLOSE : StateEnum.OPENING;

            case StateEnum.OPEN:
                return StateEnum.CLOSING;

            case StateEnum.CLOSING:
                return StateEnum.CLOSE;

            case StateEnum.OPENING:
            case StateEnum.FORCE_OPEN:
            case StateEnum.DESTRUCT:
                return StateEnum.OPEN;

            default:
                return StateEnum.CLOSE;
        }
    }
}

