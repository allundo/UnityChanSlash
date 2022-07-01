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
        if (trailTarget != null) Track(trailTarget.position - transform.position);
    }

    private void LookAt(Vector3 lookAt)
    {
        currentLookAt = lookAt;
        transform.LookAt(lookAt);
    }

    private void Track(Vector3 target, float rate = 0.025f)
    {
        LookAt(currentLookAt * (1.0f - rate) + target * rate);
    }

    public void SetRange(float range, float duration = 1f)
    {
        float from = spotLight.range;
        DOVirtual.Float(from, range, duration, value => spotLight.range = value).Play();
    }

    public void SetTrackTarget(Transform targetTf)
    {
        trailTarget = targetTf;
    }
}