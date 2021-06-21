using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyLifeGauge : MonoBehaviour
{
    [SerializeField] private GaugeAlpha greenGauge;
    [SerializeField] private GaugeAlpha blackGauge;
    [SerializeField] private GaugeAlpha redGauge;
    [SerializeField] private AlphaRawImage brightness;


    public void SetAlpha(float alpha)
    {
        greenGauge.SetAlpha(alpha);
        redGauge.SetAlpha(alpha);
        blackGauge.SetAlpha(alpha);
        brightness.SetAlpha(alpha);
    }
}
