using UnityEngine;

public class DoorControlN : DoorControl
{
    private Vector3 vecL;

    public DoorControlN SetDir(IDirection dir)
    {
        vecL = dir.Left.LookAt * 0.75f;
        return this;
    }

    protected override void Awake()
    {
        base.Awake();
        SetDir(Direction.north);
    }

    protected override Vector3 VecL => vecL;
    protected override Vector3 VecR => -vecL;
}
