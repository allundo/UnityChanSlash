using UnityEngine;

public class SideCamera : MonoBehaviour
{
    private Transform lookAt = default;
    private Vector3 followOffset = default;

    public Material shader = default;

    private Camera cam;

    public void Enable()
    {
        cam.enabled = true;
    }

    public void Disable()
    {
        cam.enabled = false;
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();

        Disable();
    }

    public void SetTarget(ThirdPersonCamera camera)
    {
        Disable();

        lookAt = camera.lookAt;
        followOffset = camera.followOffset;
    }

    public void SetRightSide(Transform cameraTf)
    {
        SetSideCamera(cameraTf, true);
    }

    public void SetLeftSide(Transform cameraTf)
    {
        SetSideCamera(cameraTf, false);
    }

    public void SetSideCamera(Transform cameraTf, bool isRight)
    {
        Vector3 cameraLocalPos = Quaternion.Euler(0, (isRight ? 90 : -90), 0) * (cameraTf.position - lookAt.position);
        Vector3 localOffset = -new Vector3(cameraLocalPos.x, 0, cameraLocalPos.z).normalized * followOffset.magnitude;

        transform.position = lookAt.position + cameraLocalPos;
        transform.LookAt(lookAt.position + localOffset);
    }

}