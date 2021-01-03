
using UnityEngine;

public class DoorHControl : DoorControl
{
    protected override Vector3 VecR => new Vector3(0.75f, 0, 0);
    protected override Vector3 VecL => new Vector3(-0.75f, 0, 0);
}