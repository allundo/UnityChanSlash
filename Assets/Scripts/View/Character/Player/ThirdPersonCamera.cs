using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] public Transform lookAt;
    [SerializeField] public float fieldOfViewP = 40f;
    [SerializeField] public float fieldOfViewL = 30f;
    [SerializeField] public Vector3 followOffsetP = new Vector3(0, 0, 6.3f);
    [SerializeField] public Vector3 followOffsetL = new Vector3(0, 0, 4f);
    [SerializeField] public Vector3 cameraPositionP = new Vector3(0, 9f, -9f);
    [SerializeField] public Vector3 cameraPositionL = new Vector3(0, 8f, -8f);
    [SerializeField] private SideCamera sideCamera = default;
    [SerializeField] private RawImage crossFade = default;
    [SerializeField] private ScreenRotateHandler rotate = default;

    public float fieldOfView { get; private set; }
    public Vector3 followOffset { get; private set; }
    public Vector3 cameraPosition { get; private set; }

    private RenderTexture renderTexture;

    private Camera cam = default;

    private void Start()
    {
        cam = Camera.main;

        ResetRenderSettings(DeviceOrientation.Portrait);
        crossFade.enabled = false;

        rotate.Orientation.Subscribe(orientation => ResetRenderSettings(orientation));
    }


    void LateUpdate()
    {
        transform.position = lookAt.position + lookAt.rotation * cameraPosition;

        transform.LookAt(lookAt.position + lookAt.rotation * followOffset);
    }

    public void ResetRenderSettings(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                fieldOfView = fieldOfViewP;
                followOffset = followOffsetP;
                cameraPosition = cameraPositionP;
                break;

            case DeviceOrientation.LandscapeRight:
                fieldOfView = fieldOfViewL;
                followOffset = followOffsetL;
                cameraPosition = cameraPositionL;
                break;
        }

        cam.fieldOfView = fieldOfView;
        sideCamera.SetTarget(this);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        crossFade.texture = renderTexture;
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
        DOVirtual.Float(1.0f, 0.0f, 0.2f, value => SetFadeAlpha(value)).Play();
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