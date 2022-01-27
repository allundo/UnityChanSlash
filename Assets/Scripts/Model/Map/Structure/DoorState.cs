using UnityEngine;
using UniRx;

public class DoorState : MonoBehaviour
{
    public enum StateEnum
    {
        OPEN,
        CLOSE,
        OPENING,
        CLOSING,
    }

    protected IReactiveProperty<StateEnum> state = new ReactiveProperty<StateEnum>(StateEnum.CLOSE);
    public IReadOnlyReactiveProperty<StateEnum> State => state;
    private bool isLocked = false;

    public bool IsOpen => State.Value == StateEnum.OPEN || State.Value == StateEnum.OPENING;
    public bool IsControllable => State.Value == StateEnum.OPEN && !IsCharacterOn || State.Value == StateEnum.CLOSE;

    public bool IsCharacterOn => onCharacterDest != null;
    public MobStatus onCharacterDest = null;

    public void TransitionNext()
    {
        state.Value = GetNextState();
    }

    public void Handle()
    {
        if (IsControllable) TransitionNext();
    }

    private StateEnum GetNextState()
    {
        switch (State.Value)
        {
            case StateEnum.CLOSE:
                return isLocked ? StateEnum.CLOSE : StateEnum.OPENING;

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

