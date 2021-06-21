using UnityEngine;

public class EnemyLifeGauge : MonoBehaviour
{
    [SerializeField] private GaugeAlpha greenGauge = default;
    [SerializeField] private GaugeAlpha blackGauge = default;
    [SerializeField] private GaugeAlpha redGauge = default;
    [SerializeField] private AlphaRawImage brightness = default;

    private float lifeMax = 1.0f;

    public void SetAlpha(float alpha)
    {
        greenGauge.SetAlpha(alpha);
        redGauge.SetAlpha(alpha);
        blackGauge.SetAlpha(alpha);
        brightness.SetAlpha(alpha);
    }

    public void OnEnemyChange(float life, float lifeMax)
    {
        this.lifeMax = lifeMax;
        float lifeRatio = life / lifeMax;

        greenGauge.SetGauge(lifeRatio);
        redGauge.SetGauge(lifeRatio);
        blackGauge.SetGauge(lifeRatio);
        brightness.SetGauge(lifeRatio);
    }

    public void OnLifeChange(float life)
    {
        float lifeRatio = life / lifeMax;

        greenGauge.UpdateGauge(lifeRatio);
        redGauge.UpdateGauge(lifeRatio);
        blackGauge.SetGauge(lifeRatio);
        brightness.SetGauge(lifeRatio);
    }
}
