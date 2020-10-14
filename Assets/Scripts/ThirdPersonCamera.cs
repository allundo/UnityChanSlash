using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform lookAt;
    public Vector3 followOffset = default;
    public Vector3 cameraPosition = default;

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        this.cam = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = lookAt.position + lookAt.rotation * cameraPosition;

        transform.LookAt(lookAt.position + lookAt.rotation * followOffset);
    }
}