using UnityEngine;
using UniRx;

public class LockedDoorControl : DoorControl
{
    protected LockControl lockControl = default;

    private Vector3 vecL;

    public LockedDoorControl SetDir(IDirection dir)
    {
        vecL = dir.Left.LookAt * 0.75f;
        return this;
    }

    protected override void Awake()
    {
        base.Awake();

        lockControl = this.transform.GetChild(2).GetComponent<LockControl>();
        SetDir(Direction.north);
    }

    protected override void Start()
    {
        base.Start();
        handleState.LockedState.Subscribe(isLocked => Lock(isLocked)).AddTo(this);
    }

    protected override Vector3 VecL => vecL;
    protected override Vector3 VecR => -vecL;

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
        lockControl.ForceBreak();
        base.ForceOpen();
    }

    protected override void ForceBreak()
    {
        lockControl.ForceBreak();
        base.ForceBreak();
    }
}
