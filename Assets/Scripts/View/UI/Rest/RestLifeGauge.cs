using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestLifeGauge : MonoBehaviour
{
    [SerializeField] private Gauge greenGauge = default;
    [SerializeField] private Image frameImage = default;
    [SerializeField] private TextMeshProUGUI lifeText = default;
    [SerializeField] private TextMeshProUGUI hpText = default;

    public void OnLifeChange(float life, float lifeMax)
    {
        float lifeRatio = life / lifeMax;

        UpdateLifeText(life, lifeMax);
        greenGauge.SetGauge(lifeRatio);
    }

    public void UpdateLifeText(float life, float lifeMax)
    {
        int hp = life > 0.0f ? (int)(life * 10) : 0;
        lifeText.text = hp + " / " + (int)(lifeMax * 10);
    }

    public void Enable()
    {
        greenGauge.Enable();
        frameImage.enabled = lifeText.enabled = hpText.enabled = true;
    }

    public void Disable()
    {
        greenGauge.Disable();
        frameImage.enabled = lifeText.enabled = hpText.enabled = false;
    }
}
