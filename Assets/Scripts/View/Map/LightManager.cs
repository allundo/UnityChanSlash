using UnityEngine;
using DG.Tweening;

public class LightManager : MonoBehaviour
{
    [SerializeField] private Light[] directionalLights = default;
    [SerializeField] private Light spotLight = default;

    void Awake()
    {
        spotLight.enabled = false;
        spotLight.spotAngle = 0f;
    }

    private void SpotLightInit(Vector3 pos, float angle)
    {
        spotLight.transform.position = pos;
        spotLight.enabled = true;
        spotLight.range = pos.y / Mathf.Cos(angle) + 1f;
    }

    public Tween DirectionalFade(float from, float to, float duration)
    {
        return DOVirtual.Float(
            from,
            to,
            duration,
            value => directionalLights.ForEach(light => light.intensity = value)
        );

    }

    public Tween SpotFadeIn(Vector3 pos, float from, float to, float duration)
    {
        return DOTween.Sequence()
            .AppendCallback(() => SpotLightInit(pos, to))
            .Join(DOVirtual.Float(from, to, duration, value => spotLight.spotAngle = value))
            .Join(DOVirtual.Float(0f, 1f, duration, value => spotLight.intensity = value));
    }

    public Tween SpotFadeOut(float from, float duration)
    {
        return DOTween.Sequence()
            .Join(DOVirtual.Float(from, 1f, duration, value => spotLight.spotAngle = value))
            .Join(DOVirtual.Float(1f, 0f, duration, value => spotLight.intensity = value))
            .AppendCallback(() => spotLight.enabled = false);
    }
}
