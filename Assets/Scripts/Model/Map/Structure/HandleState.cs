using UniRx;
using System;

public interface IOpenState
{
    bool IsOpen { get; }
    void Open();
}

public interface IHandleState : IOpenState
{
    IReadOnlyReactiveProperty<HandleState.StateEnum> State { get; }
    IObservable<bool> LockedState { get; }

    bool IsControllable { get; }
    bool IsLocked { get; }

    void TransitToNextState();
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
        FORCE_UNLOCK,
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

    public void Unlock()
    {
        lockType.Value = ItemType.Null;
        state.Value = StateEnum.FORCE_UNLOCK;
    }

    protected virtual ActiveMessageData lockedMessage
        => new ActiveMessageData("鍵がかかっている…");

    private StateEnum GetNextState()
    {
        switch (State.Value)
        {
            case StateEnum.CLOSE:
                if (IsLocked)
                {
                    ActiveMessageController.Instance.InputMessageData(lockedMessage);
                    return StateEnum.CLOSE;
                }
                return StateEnum.OPENING;

            case StateEnum.OPEN:
                return StateEnum.CLOSING;

            case StateEnum.CLOSING:
            case StateEnum.FORCE_UNLOCK:
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

