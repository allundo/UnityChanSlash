using UnityEngine;
using DG.Tweening;

public class LightManager : MonoBehaviour
{
    [SerializeField] private Light directionalLight = default;
    [SerializeField] private Light pointLight = default;
    [SerializeField] private Light spotLight = default;

    private float directionalIntensity;
    private float pointIntensity;

    void Awake()
    {
        spotLight.enabled = false;
        spotLight.spotAngle = 0f;
        directionalIntensity = directionalLight.intensity;
        pointIntensity = pointLight.intensity;
    }

    private void SpotLightInit(Vector3 pos, float angle)
    {
        spotLight.transform.position = pos;
        spotLight.enabled = true;
        spotLight.range = pos.y / Mathf.Cos(angle) + 1f;
    }

    public Tween DirectionalFadeIn(float duration)
        => Fade(directionalLight, 0.2f, directionalIntensity, duration);

    public Tween DirectionalFadeOut(float duration)
        => Fade(directionalLight, directionalIntensity, 0.2f, duration);

    public Tween PointFadeIn(float duration)
        => Fade(pointLight, 0f, pointIntensity, duration);

    public Tween PointFadeOut(float duration)
        => Fade(pointLight, pointIntensity, 0f, duration);

    private Tween Fade(Light light, float from, float to, float duration)
        => DOVirtual.Float(from, to, duration, value => light.intensity = value);

    public Tween SpotFadeIn(Vector3 pos, float from, float to, float duration)
    {
        return DOTween.Sequence()
            .AppendCallback(() => SpotLightInit(pos, to))
            .Join(DOVirtual.Float(from, to, duration, value => spotLight.spotAngle = value))
            .Join(Fade(spotLight, 0f, 1f, duration));
    }

    public Tween SpotFadeOut(float from, float duration)
    {
        return DOTween.Sequence()
            .Join(DOVirtual.Float(from, 1f, duration, value => spotLight.spotAngle = value))
            .Join(DOVirtual.Float(1f, 0f, duration, value => spotLight.intensity = value))
            .AppendCallback(() => spotLight.enabled = false);
    }
}
