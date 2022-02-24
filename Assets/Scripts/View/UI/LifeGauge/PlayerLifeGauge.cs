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

    [SerializeField] private AudioSource lifeMaxSound = null;
    [SerializeField] private AudioSource smallHealSound = null;

    private RectTransform rectTransform;
    private HealEffect healEffect;

    private Tween shakeTween = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        healEffect = new HealEffect(healImage, healVfx, rectTransform.sizeDelta.x);
    }

    public void UpdateLife(float life, float lifeMax, bool isFlash)
    {
        UpdateLifeText(life, lifeMax);
        UpdateGauge(life / lifeMax, isFlash);
    }

    private void UpdateGauge(float lifeRatio, bool isFlash)
    {
        greenGauge.UpdateGauge(lifeRatio, isFlash);
        redGauge.UpdateGauge(lifeRatio);
    }

    /// <summary>
    /// Play life gauge effect on heal
    /// </summary>
    /// <param name="healRatio">Normalized heal ratio to the life max</param>
    /// <param name="lifeRatio">Normalized life ratio after healing to the life max</param>
    public void OnHeal(float healRatio, float lifeRatio)
    {
        UpdateGauge(lifeRatio, true);
        healEffect.PlayEffect(healRatio * 0.5f, lifeRatio);
    }

    public void OnNoEffectHeal(float heal, float prevLife, float lifeMax)
    {
        float life = heal + prevLife;

        UpdateGauge(life / lifeMax, true);

        bool isHealVisible = (int)(life * 10f) > (int)(prevLife * 10f);
        if (isHealVisible) smallHealSound.PlayEx();
    }

    public void OnLifeMax()
    {
        lifeMaxSound.PlayEx();
    }

    public void OnDamage(float damageRatio, float lifeRatio)
    {
        if (damageRatio < 0.000001f)
        {
            UpdateGauge(lifeRatio, false);
            return;
        }

        healEffect.KillEffect();

        UpdateGauge(lifeRatio, true);

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
}
