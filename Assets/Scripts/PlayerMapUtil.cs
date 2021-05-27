using UnityEngine;

public class PlayerMapUtil : MapUtil
{
    [SerializeField] protected ThirdPersonCamera mainCamera = default;

    public void RedrawHidePlates()
    {
        MapRenderer.Instance.RedrawHidePlates(CurrentVec3Pos);
    }

    public void MoveHidePlates()
    {
        MapRenderer.Instance.MoveHidePlates(CurrentVec3Pos);
    }

    public override void TurnLeft()
    {
        base.TurnLeft();
        mainCamera.TurnLeft();
    }

    public override void TurnRight()
    {
        base.TurnRight();
        mainCamera.TurnRight();
    }

    public void ResetCamera()
    {
        mainCamera.ResetCamera();
    }
}