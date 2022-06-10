using UnityEngine;
using UniRx;
using DG.Tweening;

public interface IHandleStructure
{
    void Handle();
}

public abstract class HandleStructure : MonoBehaviour, IHandleStructure
{
    protected IHandleState handleState;

    protected Tween movingControl;

    public void KillTween()
    {
        movingControl?.Kill();
    }

    public void CompleteTween()
    {
        movingControl?.Complete(true);
    }

    protected virtual void Start()
    {
        handleState.State.Subscribe(state => OnStateChange(state)).AddTo(this);
    }

    public IHandleStructure SetState(IHandleState state)
    {
        this.handleState = state;
        return this;
    }

    public void Handle()
    {
        if (handleState.IsControllable) handleState.TransitToNextState();
    }

    private void OnStateChange(HandleState.StateEnum state)
    {
        switch (state)
        {
            case HandleState.StateEnum.OPENING:
                movingControl = OpenTween.Play();
                break;

            case HandleState.StateEnum.CLOSING:
                movingControl = CloseTween.Play();
                break;
        }
    }

    private Tween OpenTween => GetDoorHandle(true);
    private Tween CloseTween => GetDoorHandle(false);

    protected abstract Tween GetDoorHandle(bool isOpen);
}
