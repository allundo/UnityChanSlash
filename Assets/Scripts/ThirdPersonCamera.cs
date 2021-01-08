using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
        SetTurnCamera();
        sideCamera.SetRightSide(transform);
    }

    public void TurnLeft()
    {
        SetTurnCamera();
        sideCamera.SetLeftSide(transform);
    }

    private void SetFadeOut()
    {
        DOVirtual.Float(1.0f, 0.0f, 0.25f, value => SetFadeAlpha(value));
    }

    private void SetFadeAlpha(float value)
    {
        Color temp = crossFade.color;
        crossFade.color = new Color(temp.r, temp.g, temp.b, value);
    }

    private void SetTurnCamera()
    {
        cam.targetTexture = renderTexture;
        crossFade.enabled = true;
        SetFadeOut();

        sideCamera.Enable();
    }

    public void ResetCamera()
    {
        cam.targetTexture = null;
        crossFade.enabled = false;
        SetFadeAlpha(1.0f);

        sideCamera.Disable();
    }
}