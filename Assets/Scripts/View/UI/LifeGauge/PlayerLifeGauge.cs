using DG.Tweening;
using UnityEngine;
using TMPro;
using Coffee.UIExtensions;

public class PlayerLifeGauge : MonoBehaviour
{
    [SerializeField] private Gauge greenGauge = default;
    [SerializeField] private Gauge redGauge = default;
    [SerializeField] private TextMeshProUGUI lifeText = default;
    [SerializeField] private SkewedImage healImage = default;
    [SerializeField] private UIParticle healVfx = default;

    private RectTransform rectTransform;
    private HealEffect healEffect;

    private Tween shakeTween = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        healEffect = new HealEffect(healImage, healVfx, rectTransform.sizeDelta.x);
    }

    public void OnLifeChange(float life, float lifeMax)
    {
        float lifeRatio = life / lifeMax;

        UpdateLifeText(life, lifeMax);
        greenGauge.UpdateGauge(lifeRatio);
        redGauge.UpdateGauge(lifeRatio);
    }

    public void OnHeal(float healRatio, float lifeRatio)
    {
        if (lifeRatio != 1f) greenGauge.color = new Color(1, 1, 1);
        healEffect.PlayEffect(healRatio * 0.5f, lifeRatio);
    }

    public void OnDamage(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        healEffect.KillEffect();
        greenGauge.color = new Color(1, 1, 1);

        shakeTween?.Kill();
        shakeTween = DamageShake(damageRatio);
    }

    public void UpdateLifeText(float life, float lifeMax)
    {
        int hp = life > 0.0f ? (int)(life * 10) : 0;
        lifeText.text = hp + " / " + (int)(lifeMax * 10);
    }

    private Tween DamageShake(float damageRatio)
    {
        if (damageRatio <= 0.0f) return null;
        return rectTransform.DOShakeAnchorPos(2 * damageRatio, 100 * damageRatio, (int)(300 * damageRatio)).Play();
    }

    protected void Play(AudioSource src)
    {
        if (src != null)
        {
            src.Play();
        }
    }
}