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

    public Tween SpotFade(Vector3 pos, float from, float to, float duration)
    {
        return DOTween.Sequence()
            .AppendCallback(() => SpotLightInit(pos, to))
            .Append(DOVirtual.Float(from, to, duration, value => spotLight.spotAngle = value));
    }

    public Tween SpotFadeOut(float from, float duration)
    {
        return DOTween.Sequence()
            .Append(DOVirtual.Float(from, 0f, duration, value => spotLight.spotAngle = value))
            .AppendCallback(() => spotLight.enabled = false);
    }
}
