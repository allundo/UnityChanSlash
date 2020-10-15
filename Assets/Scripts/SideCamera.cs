using UnityEngine;

public class SideCamera : MonoBehaviour
{
    private Transform lookAt = default;
    private Vector3 followOffset = default;

    public Material shader = default;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        enabled = false;
    }

    public void SetTarget(ThirdPersonCamera camera)
    {
        enabled = false;

        lookAt = camera.lookAt;
        followOffset = camera.followOffset;
    }

    public void SetRightSide(Transform cameraTf)
    {
        Vector3 cameraLocalPos = Quaternion.Euler(0, 90, 0) * (cameraTf.position - lookAt.position);
        Vector3 localOffset = -new Vector3(cameraLocalPos.x, 0, cameraLocalPos.z).normalized * followOffset.magnitude;

        transform.position = lookAt.position + cameraLocalPos;
        transform.LookAt(lookAt.position + localOffset);
        enabled = true;
    }

    public void SetLeftSide(Transform cameraTf)
    {
        Vector3 cameraLocalPos = Quaternion.Euler(0, -90, 0) * (cameraTf.position - lookAt.position);
        Vector3 localOffset = -new Vector3(cameraLocalPos.x, 0, cameraLocalPos.z).normalized * followOffset.magnitude;

        transform.position = lookAt.position + cameraLocalPos;
        transform.LookAt(lookAt.position + localOffset);
        enabled = true;
    }

}