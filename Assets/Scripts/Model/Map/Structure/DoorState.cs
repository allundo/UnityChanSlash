using UniRx;
using System;

public class DoorState
{
    public DoorState() : this(ItemType.Null) { }
    public DoorState(ItemType type)
    {
        lockType = new ReactiveProperty<ItemType>(type);
    }

    public enum StateEnum
    {
        OPEN,
        CLOSE,
        OPENING,
        CLOSING,
    }

    protected IReactiveProperty<StateEnum> state = new ReactiveProperty<StateEnum>(StateEnum.CLOSE);
    public IReadOnlyReactiveProperty<StateEnum> State => state;

    protected IReactiveProperty<ItemType> lockType;
    public IObservable<bool> LockedState => lockType.SkipLatestValueOnSubscribe().Select(type => type != ItemType.Null);

    public bool IsLocked => lockType.Value != ItemType.Null;

    public bool Unlock(ItemType key)
    {
        if (key != lockType.Value) return false;

        lockType.Value = ItemType.Null;
        return true;
    }

    public bool IsOpen => State.Value == StateEnum.OPEN || State.Value == StateEnum.OPENING;
    public bool IsControllable => State.Value == StateEnum.OPEN && !IsCharacterOn || State.Value == StateEnum.CLOSE;

    public bool IsCharacterOn => onCharacterDest != null;
    public IStatus onCharacterDest = null;

    protected void TransitionNext()
    {
        state.Value = GetNextState();
    }

    public void TransitToNextState()
    {
        if (IsControllable) state.Value = GetNextState();
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
                return StateEnum.OPEN;

            default:
                return StateEnum.CLOSE;
        }
    }
}

