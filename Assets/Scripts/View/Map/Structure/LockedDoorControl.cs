using UniRx;

public class LockedDoorControl : DoorControlN
{
    protected LockControl lockControl = default;

    protected override void Awake()
    {
        base.Awake();

        lockControl = this.transform.GetChild(2).GetComponent<LockControl>();
    }

    protected override void Start()
    {
        base.Start();
        handleState.LockedState.Subscribe(isLocked => Lock(isLocked)).AddTo(this);
    }

    protected void Lock(bool isLocked)
    {
        if (isLocked)
        {
            lockControl.Reset();
        }
        else
        {
            lockControl.Unlock();
        }
    }

    protected override void ForceOpen()
    {
        lockControl.ForceDelete();
        base.ForceOpen();
    }

    protected override void ForceUnlock()
    {
        lockControl.ForceDelete();
        handleState.TransitToNextState();
    }

    protected override void ForceBreak()
    {
        lockControl.ForceDelete();
        base.ForceBreak();
    }
}
