using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class CameraWork : MonoBehaviour
{
    private enum State
    {
        NONE,
        TRACK,
        START,
        TRAIL
    }

    [SerializeField] private Transform tfUnityChan = default;
    [SerializeField] private RawImage crossFade = default;
    [SerializeField] private Camera secondCamera = default;

    private Camera currentCamera;
    private Camera standByCamera;
    private RenderTexture renderTexture;

    private Vector3 EyePosition => tfUnityChan.position + EyeHeight;
    private readonly Vector3 EyeHeight = new Vector3(0f, 1.35f, 0f);

    private Tween cameraWorkTween = null;
    private State state = State.NONE;
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

    void Start()
    {
        currentCamera = Camera.main;
        secondCamera.fieldOfView = currentCamera.fieldOfView;

        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        crossFade.texture = renderTexture;
        crossFade.enabled = false;

        standByCamera = secondCamera;
        standByCamera.enabled = false;
        standByCamera.targetTexture = renderTexture;
    }

    void Update()
    {
        switch (state)
        {
            case State.TRACK:
                LookAt(EyePosition);
                break;

            case State.START:
                Trail(EyePosition + new Vector3(0f, -0.4f, 0f));
                break;

            case State.TRAIL:
                Trail(EyePosition, 0.25f);
                break;
        }
    }

    private void LookAt(Vector3 lookAt)
    {
        currentLookAt = lookAt;
        currentCamera.transform.LookAt(lookAt);
        standByCamera.transform.LookAt(lookAt);
    }

    private void Trail(Vector3 target, float rate = 0.025f)
    {
        LookAt(currentLookAt * (1.0f - rate) + target * rate);
    }

    public void ToTitle(TweenCallback OnComplete = null)
    {
        OnComplete = OnComplete ?? (() => { });

        currentCamera.transform.position = new Vector3(-10f, 1.35f, 2.5f);

        currentCamera.transform.DOMove(new Vector3(0f, 1.35f, 2.5f), 1.2f)
            .OnComplete(OnComplete)
            .Play();
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
        state = State.START;
        return currentCamera.transform
            .DOMove(EyePosition + new Vector3(0f, -0.4f, 1.8f), 1f)
            .OnPlay(() => cameraWorkTween?.Kill());
    }

    public void StartCameraWork()
    {
        state = State.TRACK;
        standByCamera.transform.position = currentCamera.transform.position;
        angleChangeLoop = StartCoroutine(AngleChangeLoop());
    }

    public void StopCameraWork()
    {
        cameraWorkTween?.Kill();
        StopCoroutine(angleChangeLoop);
        state = State.NONE;
    }

    public void StartTrail()
    {
        state = State.TRAIL;
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
