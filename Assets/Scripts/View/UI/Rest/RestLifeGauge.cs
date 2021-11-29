using DG.Tweening;
using UnityEngine;
using TMPro;

public class RestLifeGauge : MonoBehaviour
{
    [SerializeField] private Gauge greenGauge = default;
    [SerializeField] private TextMeshProUGUI lifeText = default;

    public void OnLifeChange(float life, float lifeMax)
    {
        float lifeRatio = life / lifeMax;

        UpdateLifeText(life, lifeMax);
        greenGauge.UpdateGauge(lifeRatio);
    }

    public void OnHeal(float healRatio, float lifeRatio)
    {
        if (lifeRatio != 1f) greenGauge.color = new Color(1, 1, 1);
    }

    public void UpdateLifeText(float life, float lifeMax)
    {
        int hp = life > 0.0f ? (int)(life * 10) : 0;
        lifeText.text = hp + " / " + (int)(lifeMax * 10);
    }
}