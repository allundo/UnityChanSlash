using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

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

    public float fieldOfView { get; private set; }
    public Vector3 followOffset { get; private set; }
    public Vector3 cameraPosition { get; private set; }

    private RenderTexture renderTexture;
    private Texture2D screenShot;

    private Camera cam = default;

    public static readonly Vector2 viewPortAspect = new Vector2(1920f, 1080f);
    public static readonly float aspect = viewPortAspect.x / viewPortAspect.y;
    public static float ScreenAspect => Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
    public static float Margin => Mathf.Clamp01(1f - aspect / ScreenAspect) * 0.5f;

    private void Start()
    {
        cam = Camera.main;

        ResetRenderSettings(DeviceOrientation.Portrait);
        crossFade.enabled = false;
    }

    void LateUpdate()
    {
        transform.position = lookAt.position + lookAt.rotation * cameraPosition;

        transform.LookAt(lookAt.position + lookAt.rotation * followOffset);
    }

    public void ResetRenderSettings(DeviceOrientation orientation)
    {
        Rect viewPortRect = new Rect(0f, 0f, 1f, 1f);
        float margin = Margin;

        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                fieldOfView = fieldOfViewP;
                followOffset = followOffsetP;
                cameraPosition = cameraPositionP;

                viewPortRect = new Rect(0f, margin, 1f, 1f - margin * 2f);
                break;

            case DeviceOrientation.LandscapeRight:
                fieldOfView = fieldOfViewL;
                followOffset = followOffsetL;
                cameraPosition = cameraPositionL;

                viewPortRect = new Rect(margin, 0f, 1f - margin * 2f, 1f);
                break;
        }

        cam.fieldOfView = fieldOfView;
        sideCamera.SetTarget(this);

        cam.rect = sideCamera.rect = viewPortRect;
        renderTexture?.Release();

        // Enable stencil by setting depth = 24
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        crossFade.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        crossFade.texture = renderTexture;

        if (screenShot != null) Destroy(screenShot);
        screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
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
        ResetCrossFade();
        sideCamera.Disable();
    }

    public void ResetCrossFade()
    {
        crossFade.texture = renderTexture;
        crossFade.enabled = false;
        SetFadeAlpha(1.0f);
        crossFade.transform.SetSiblingIndex(1);
    }

    public void StopScreen(int screenCoverIndex = 8)
    {
        StartCoroutine(DisplayScreenShot(screenCoverIndex));
    }

    private IEnumerator DisplayScreenShot(int screenCoverIndex = 8)
    {
        // Wait for rendering before read pixels
        yield return new WaitForEndOfFrame();

        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();
        yield return new WaitForEndOfFrame();

        crossFade.texture = screenShot;
        crossFade.enabled = true;

        // Set cross fade Image to forefront
        crossFade.transform.SetSiblingIndex(screenCoverIndex - 1);
    }
}
