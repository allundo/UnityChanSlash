using UnityEngine;
using UniRx;
using DG.Tweening;

public interface IHandleStructure
{
    void Handle();
}

[RequireComponent(typeof(Collider))]
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

    public IHandleStructure SetTileState(IHandleTile tile)
    {
        handleState = tile.state as IHandleState;
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

            case HandleState.StateEnum.FORCE_OPEN:
                KillTween();
                ForceOpen();
                break;

            case HandleState.StateEnum.FORCE_UNLOCK:
                ForceUnlock();
                break;

            case HandleState.StateEnum.DESTRUCT:
                ForceBreak();
                break;
        }
    }

    private Tween OpenTween => GetDoorHandle(true);
    private Tween CloseTween => GetDoorHandle(false);

    protected abstract Tween GetDoorHandle(bool isOpen);
    protected abstract void ForceOpen();
    protected virtual void ForceUnlock() { }
    protected virtual void ForceBreak() { }
}
