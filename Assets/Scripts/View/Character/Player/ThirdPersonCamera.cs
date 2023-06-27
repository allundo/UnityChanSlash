using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] public float fieldOfViewP = 40f;
    [SerializeField] public float fieldOfViewL = 30f;
    [SerializeField] public Vector3 followOffsetP = new Vector3(0, 0, 6.3f);
    [SerializeField] public Vector3 followOffsetL = new Vector3(0, 0, 4f);
    [SerializeField] public Vector3 cameraPositionP = new Vector3(0, 9f, -9f);
    [SerializeField] public Vector3 cameraPositionL = new Vector3(0, 8f, -8f);
    [SerializeField] private SideCamera sideCamera = default;
    [SerializeField] private RawImage crossFade = default;
    [SerializeField] private Light pointLight = default;
    [SerializeField] public Vector3 pointLightOffset = new Vector3(0, 4f, 0);

    public Vector3 followOffset { get; private set; }
    public Vector3 position { get; private set; }

    public Rect rect { get { return cam.rect; } private set { cam.rect = value; } }
    public float fieldOfView { get { return cam.fieldOfView; } private set { cam.fieldOfView = value; } }

    public Transform lookAt { get; private set; }
    public void SetLookAt(Transform lookAt) => this.lookAt = lookAt;

    private float ampFactor = 0f;
    private float ampSign = 1f;
    private Tween ampTween;
    public void Amplify(float duration = 1f, float power = 0.01f)
    {
        ampTween?.Kill();
        ampTween = DOVirtual.Float(power, 0f, duration, value => ampFactor = value).Play();
    }
    public void StopAmplify() => ampTween?.Complete();

    private RenderTexture renderTexture;
    private Texture2D screenShot;

    private Camera cam = default;
    private FloorMaterialsData floorMaterialsData;

    public static readonly Vector2 viewPortAspect = new Vector2(1920f, 1080f);
    public static readonly float aspect = viewPortAspect.x / viewPortAspect.y;
    public static float ScreenAspect => Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
    public static float Margin => Mathf.Clamp01(1f - aspect / ScreenAspect) * 0.5f;

    void Awake()
    {
        cam = Camera.main;
        floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;
        ResetRenderSettings(DeviceOrientation.Portrait);
        crossFade.enabled = false;
    }

    void LateUpdate()
    {
        transform.position = lookAt.position + lookAt.rotation * position;

        transform.LookAt(lookAt.position + lookAt.rotation * followOffset);

        ApplyAmp();
    }

    private void ApplyAmp()
    {
        if (ampFactor == 0f) return;


        transform.position += new Vector3(0, 25f * ampFactor * ampSign, 0);
        transform.Rotate(ampFactor * Random.insideUnitSphere * 90f);
        ampSign *= -1f;
    }

    public void SwitchFloor(int floor)
    {
        pointLight.color = floorMaterialsData.Param(floor - 1).pointLightColor;
        UpdatePointLightPos();
    }

    private void UpdatePointLightPos()
    {
        LateUpdate();
        pointLight.transform.position = lookAt.position + pointLightOffset;
    }

    void OnDestroy()
    {
        renderTexture?.Release();
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
                position = cameraPositionP;

                viewPortRect = new Rect(0f, margin, 1f, 1f - margin * 2f);
                break;

            case DeviceOrientation.LandscapeRight:
                fieldOfView = fieldOfViewL;
                followOffset = followOffsetL;
                position = cameraPositionL;

                viewPortRect = new Rect(margin, 0f, 1f - margin * 2f, 1f);
                break;
        }

        rect = viewPortRect;
        sideCamera.CopyParams(this);

        renderTexture?.Release();

        // Enable stencil by setting depth = 24
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        crossFade.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        crossFade.texture = renderTexture;

        if (screenShot != null) Destroy(screenShot);
        screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // Reset point light position
        UpdatePointLightPos();
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

    public IEnumerator DisplayScreenShot(int screenCoverIndex = 8)
    {
        var waitForEndOfFrame = new WaitForEndOfFrame();

        // Wait for rendering before read pixels
        yield return waitForEndOfFrame;

        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();
        yield return waitForEndOfFrame;

        crossFade.texture = screenShot;
        crossFade.enabled = true;

        // Set cross fade Image to forefront
        crossFade.transform.SetSiblingIndex(screenCoverIndex - 1);
        yield return waitForEndOfFrame;
    }
}
