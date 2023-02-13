using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using UniRx;
using Random = UnityEngine.Random;

public class CameraWork : MonoBehaviour
{
    [SerializeField] private Transform tfUnityChan = default;
    [SerializeField] private RawImage crossFade = default;
    [SerializeField] private Camera secondCamera = default;
    [SerializeField] private PortraitRotateHandler rotate = default;

    private Camera currentCamera;
    private Camera standByCamera;
    private RenderTexture renderTexture;

    private Vector3 EyePosition => tfUnityChan.position + EyeHeight;
    private readonly Vector3 EyeHeight = new Vector3(0f, 1.35f, 0f);

    private Tween cameraWorkTween = null;

    private Action<Vector3> TrackAction = eyePosition => { };
    private Vector3 currentLookAt = Vector3.zero;

    private Coroutine angleChangeLoop = null;

    private void CameraTween(float duration = 10f, float blendDuration = 2.5f)
    {
        SetFadeOut(blendDuration);

        float distance = Random.Range(2f, 3f);

        Vector3 diff = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) * new Vector3(0f, distance * 0.5f, 0f);
        Quaternion angle = Quaternion.Euler(0f, Random.Range(-60f, 60f), 0f);

        Vector3 startPos = angle * (EyePosition + new Vector3(0f, 0f, distance) + diff);
        Vector3 endPos = angle * (EyePosition + new Vector3(0f, 0f, distance) - diff);

        currentCamera.transform.position = startPos;
        cameraWorkTween = currentCamera.transform.DOMove(endPos, duration).SetEase(Ease.Linear).Play();
    }

    void Awake()
    {
        currentCamera = Camera.main;
        secondCamera.fieldOfView = currentCamera.fieldOfView;

        crossFade.enabled = false;

        standByCamera = secondCamera;
        standByCamera.enabled = false;

        ResetOrientation();
    }

    void Start()
    {
        rotate.Orientation.Subscribe(_ => ResetOrientation()).AddTo(this);
    }

    private void ResetOrientation()
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        crossFade.texture = renderTexture;
        standByCamera.targetTexture = renderTexture;
    }
    void OnDestroy()
    {
        renderTexture?.Release();
    }

    void Update()
    {
        TrackAction(EyePosition);
    }

    private void Track(Vector3 lookAt)
    {
        currentLookAt = lookAt;
        currentCamera.transform.LookAt(lookAt);
        standByCamera.transform.LookAt(lookAt);
    }

    private void Trail(Vector3 target, float rate)
    {
        Track(currentLookAt * (1.0f - rate) + target * rate);
    }

    private void TrailBody(Vector3 eyePosition) => Trail(eyePosition + new Vector3(0f, -0.4f, 0f), 0.025f);

    private void TrailEye(Vector3 eyePosition) => Trail(eyePosition, 0.25f);

    public Tween TitleTween()
    {
        return
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    currentCamera.transform.position = new Vector3(-10f, 1.35f, 2.5f);
                })
                .Append(currentCamera.transform.DOMove(new Vector3(0f, 1.35f, 2.5f), 1.2f))
                .OnComplete(StartCameraWork);
    }

    private IEnumerator AngleChangeLoop(float duration = 10f)
    {
        while (true)
        {
            yield return new WaitForSeconds(duration);
            CameraTween(duration);
        }
    }

    public Tween StartTween()
    {
        TrackAction = TrailBody;
        return currentCamera.transform
            .DOMove(EyePosition + new Vector3(0f, -0.4f, 1.8f), 1f)
            .OnPlay(() => cameraWorkTween?.Kill());
    }

    public Tween ShakeTween()
    {
        Transform tf = currentCamera.transform;
        return DOTween.Sequence()
            .Append(tf.DORotate(new Vector3(-4f, 0f, 0f), 0.05f).SetRelative(true).SetEase(Ease.Linear))
            .Append(tf.DORotate(new Vector3(6f, 0f, 0f), 0.25f).SetRelative(true).SetEase(Ease.OutQuad));
    }

    public void StartCameraWork()
    {
        TrackAction = Track;
        standByCamera.transform.position = currentCamera.transform.position;
        angleChangeLoop = StartCoroutine(AngleChangeLoop());
    }

    public void StopCameraWork()
    {
        cameraWorkTween?.Kill();
        StopCoroutine(angleChangeLoop);
        TrackAction = eyePosition => { };
    }

    public void StartTrail()
    {
        TrackAction = TrailEye;
    }

    private void SetFadeOut(float duration = 2f, float startAlpha = 1f)
    {
        Camera tmp = currentCamera;
        currentCamera = standByCamera;
        standByCamera = tmp;

        currentCamera.targetTexture = null;
        currentCamera.enabled = true;

        standByCamera.targetTexture = renderTexture;

        Color c = crossFade.color;
        crossFade.color = new Color(c.r, c.g, c.b, startAlpha);
        crossFade.enabled = true;

        DOTween
            .ToAlpha(() => crossFade.color, color => crossFade.color = color, 0f, duration)
            .OnComplete(() => standByCamera.enabled = false)
            .SetDelay(0.02f)
            .Play();
    }
}
