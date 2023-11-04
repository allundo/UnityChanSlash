using UnityEngine;
using UniRx;

public class SealableDoorControl : DoorControlN
{
    [SerializeField] protected ParticleSystem sealVfx = default;

    protected IEventObservableState eventState;

    protected override void Start()
    {
        handleState.State.Subscribe(state => OnStateChange(state)).AddTo(this);
        eventState.EventObservable.Subscribe(isEventOn => ApplySeal(isEventOn)).AddTo(this);
    }

    public override IHandleStructure SetTileState(IHandleTile tile)
    {
        handleState = tile.state as IHandleState;
        eventState = tile.state as IEventObservableState;
        return this;
    }

    protected void ApplySeal(bool isEventOn)
    {
        if (isEventOn)
        {
            sealVfx.PlayEx();
        }
        else
        {
            sealVfx.StopEmitting();
        }
    }
}
