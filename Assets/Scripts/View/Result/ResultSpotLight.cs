using UnityEngine;
using DG.Tweening;

public class ResultSpotLight : MonoBehaviour
{
    private Light spotLight;
    private Transform trailTarget = null;
    private Vector3 currentLookAt;

    void Awake()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        if (trailTarget != null) Trail(trailTarget.position);
    }

    private void LookAt(Vector3 lookAt)
    {
        currentLookAt = lookAt;
        transform.LookAt(lookAt);
    }

    private void Trail(Vector3 target, float rate = 0.01f)
    {
        LookAt(currentLookAt * (1.0f - rate) + target * rate);
    }

    public void SetAngle(float angle, float duration = 1f, Ease ease = Ease.OutQuad)
    {
        float from = spotLight.spotAngle;
        DOVirtual.Float(from, angle, duration, value => spotLight.spotAngle = value).SetEase(ease).Play();
    }

    public void SetRange(float range, float duration = 1f, Ease ease = Ease.OutQuad)
    {
        float from = spotLight.range;
        DOVirtual.Float(from, range, duration, value => spotLight.range = value).SetEase(ease).Play();
    }

    public void SetTrackTarget(Transform targetTf)
    {
        trailTarget = targetTf;
    }
}