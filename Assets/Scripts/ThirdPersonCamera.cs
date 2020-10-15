using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform lookAt;
    public Vector3 followOffset = default;
    public Vector3 cameraPosition = default;

    public SideCamera sideCamera = default;

    public RawImage crossFade = default;

    private RenderTexture renderTexture;

    private Camera cam = default;

    private void Start()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        crossFade.texture = renderTexture;
        crossFade.enabled = false;

        cam = GetComponent<Camera>();
        sideCamera.SetTarget(this);
    }

    void LateUpdate()
    {
        transform.position = lookAt.position + lookAt.rotation * cameraPosition;

        transform.LookAt(lookAt.position + lookAt.rotation * followOffset);
    }

    public void TurnRight()
    {
        crossFade.enabled = true;
        cam.targetTexture = renderTexture;
        sideCamera.SetRightSide(transform);
        DOVirtual.Float(1.0f, 0.0f, 0.3f, value =>
        {
            Color temp = crossFade.color;
            crossFade.color = new Color(temp.r, temp.g, temp.b, value);
            Debug.Log(crossFade.color);
        });
        DOVirtual.DelayedCall(0.6f, () => ResetCamera());
    }

    public void TurnLeft()
    {
        crossFade.enabled = true;
        cam.targetTexture = renderTexture;
        sideCamera.SetLeftSide(transform);
        DOVirtual.Float(1.0f, 0.0f, 0.3f, value =>
        {
            Color temp = crossFade.color;
            crossFade.color = new Color(temp.r, temp.g, temp.b, value);
            Debug.Log(crossFade.color);
        });
        DOVirtual.DelayedCall(0.6f, () => ResetCamera());

    }

    public void ResetCamera()
    {
        cam.targetTexture = null;
        crossFade.enabled = false;

        sideCamera.enabled = false;
    }
}