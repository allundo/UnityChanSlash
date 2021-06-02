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

    private StateEnum state
    {
        get
        {
            return State.Value;
        }
        set
        {
            State.Value = value;
        }
    }

    public IReactiveProperty<StateEnum> State = new ReactiveProperty<StateEnum>(StateEnum.CLOSE);
    private bool isLocked = false;

    public bool IsOpen => state == StateEnum.OPEN || state == StateEnum.OPENING;
    private bool IsControllable => state == StateEnum.OPEN || state == StateEnum.CLOSE;

    public void TransitionNext()
    {
        state = GetNextState();
    }

    public void Handle()
    {
        if (IsControllable) TransitionNext();
    }

    private StateEnum GetNextState()
    {
        switch (state)
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

