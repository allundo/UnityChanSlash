using UnityEngine;

public class EnemyLifeGauge : MonoBehaviour
{
    [SerializeField] private GaugeAlpha greenGauge = default;
    [SerializeField] private GaugeAlpha blackGauge = default;
    [SerializeField] private GaugeAlpha redGauge = default;
    [SerializeField] private AlphaRawImage brightness = default;

    [SerializeField] public float lifeRatio = 1.0f;

    public void SetAlpha(float alpha)
    {
        greenGauge.SetAlpha(alpha);
        redGauge.SetAlpha(alpha);
        blackGauge.SetAlpha(alpha);
        brightness.SetAlpha(alpha);
    }

    void Update()
    {
        greenGauge.UpdateGauge(lifeRatio);
        redGauge.UpdateGauge(lifeRatio);
        blackGauge.SetGauge(lifeRatio);
        brightness.SetGauge(lifeRatio);
    }
}
