using UnityEngine;
public class TargetTest : Target
{
    public Camera dummyCamera { private get; set; }

    protected override Camera MainCamera => dummyCamera;
}