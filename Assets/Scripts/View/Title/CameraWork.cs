using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CameraWork : MonoBehaviour
{
    [SerializeField] private Transform tfUnityChan = default;

    private Vector3 EyePosition => tfUnityChan.position + new Vector3(0f, 1.35f, 0f);

    private Tween cameraWorkTween = null;
    private void CameraTween()
    {
        float distance = Random.Range(2f, 3f);

        Vector3 diff = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) * new Vector3(0f, distance * 0.5f, 0f);
        Quaternion angle = Quaternion.Euler(0f, Random.Range(-60f, 60f), 0f);

        Vector3 startPos = angle * (EyePosition + new Vector3(0f, 0f, distance) + diff);
        Vector3 endPos = angle * (EyePosition + new Vector3(0f, 0f, distance) - diff);

        transform.position = startPos;
        cameraWorkTween = transform.DOMove(endPos, 10f).SetEase(Ease.Linear).Play();
    }

    void Update()
    {
        transform.LookAt(EyePosition);
    }

    public void ToTitle(TweenCallback OnComplete = null)
    {
        OnComplete = OnComplete ?? (() => { });

        transform.position = new Vector3(-10f, 1.35f, 2.5f);

        transform.DOMove(new Vector3(0f, 1.35f, 2.5f), 1.2f)
            .OnComplete(OnComplete)
            .Play();
    }

    private IEnumerator AngleChangeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            CameraTween();
        }
    }

    public void StartCameraWork()
    {
        StartCoroutine(AngleChangeLoop());
    }

    public void StopCameraWork()
    {
        cameraWorkTween?.Kill();
        StopCoroutine(AngleChangeLoop());
    }
}
