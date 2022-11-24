using UnityEngine;

public class SideCamera : MonoBehaviour
{
    private Transform lookAt;
    private Vector3 followOffset;
    private Vector3 position;

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

    public void CopyParams(ThirdPersonCamera camera)
    {
        Disable();
        cam.fieldOfView = camera.fieldOfView;
        cam.rect = camera.rect;
        lookAt = camera.lookAt;
        followOffset = camera.followOffset;
        position = camera.position;
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
        Vector3 cameraLocalPos = lookAt.rotation * Quaternion.Euler(0, (isRight ? 90 : -90), 0) * position;
        Vector3 localOffset = -new Vector3(cameraLocalPos.x, 0, cameraLocalPos.z).normalized * followOffset.magnitude;

        transform.position = lookAt.position + cameraLocalPos;
        transform.LookAt(lookAt.position + localOffset);
    }
}