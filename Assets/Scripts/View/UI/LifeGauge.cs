using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeGauge : MonoBehaviour
{
    [SerializeField] private Image GreenGauge = default;
    [SerializeField] private Image RedGauge = default;
    [SerializeField] private TextMeshProUGUI lifeText = default;

    private RectTransform rectTransform;

    private readonly Color32[] ratio =
    {
        new Color32(0xFF, 0xFF, 0x00, 0xFF),
        new Color32(0xa3, 0xFF, 0x00, 0xFF),
        new Color32(0x30, 0xFF, 0x00, 0xFF),
        new Color32(0x00, 0xF6, 0x3F, 0xFF),
        new Color32(0x00, 0xE9, 0x9B, 0xFF),
        new Color32(0x00, 0xE0, 0xE0, 0xFF),
    };

    private Tween redGaugeTween = null;
    private Tween greenGaugeTween = null;
    private Tween shakeTween = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Start()
    {
        GreenGauge.fillAmount = RedGauge.fillAmount = 1.0f;
        GreenGauge.color = ratio[5];
    }

    public void OnLifeChange(float life, float lifeMax)
    {
        float lifeRatio = life / lifeMax;

        UpdateLifeText(life, lifeMax);
        UpdateGreenGauge(lifeRatio);

        redGaugeTween?.Kill();
        redGaugeTween = GetRedGaugeTween(RedGauge.fillAmount, lifeRatio).Play();
    }

    public void OnDamage(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        GreenGauge.color = new Color(1, 1, 1);
        shakeTween?.Kill();
        shakeTween = GetDamageShake(damageRatio)?.Play();
    }

    public void UpdateLifeText(float life, float lifeMax)
    {
        int hp = life > 0.0f ? (int)(life * 10) : 0;
        lifeText.text = hp + " / " + (int)(lifeMax * 10);
    }

    private void UpdateGreenGauge(float lifeRatio)
    {
        GreenGauge.fillAmount = lifeRatio;

        for (float compare = 5.0f; compare >= 0.0f; compare -= 1.0f)
        {
            if (lifeRatio > compare / 6.0f)
            {
                greenGaugeTween?.Kill();
                greenGaugeTween = GreenGauge.DOColor(ratio[(int)compare], 0.5f).Play();
                break;
            }
        }
    }

    private Tween GetRedGaugeTween(float valueFrom, float valueTo, float duration = 1f)
    {
        return DOTween.To(
            () => valueFrom,
            value => RedGauge.fillAmount = value,
            valueTo,
            duration
        );
    }

    private Tween GetDamageShake(float damageRatio)
    {
        if (damageRatio <= 0.0f) return null;
        return rectTransform.DOShakeAnchorPos(2 * damageRatio, 100 * damageRatio, (int)(300 * damageRatio));
    }

}
