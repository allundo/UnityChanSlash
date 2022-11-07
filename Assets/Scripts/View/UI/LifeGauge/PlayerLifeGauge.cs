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

    private bool effectOnUpdate = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        healEffect = new HealEffect(healImage, healVfx, rectTransform.sizeDelta.x);
    }

    public void UpdateLife(float life, float lifeMax)
    {
        UpdateLifeText(life, lifeMax);
        UpdateGauge(life / lifeMax);
    }

    private void UpdateGauge(float lifeRatio)
    {
        greenGauge.UpdateGauge(lifeRatio, effectOnUpdate);
        redGauge.UpdateGauge(lifeRatio, effectOnUpdate);
        effectOnUpdate = false;
    }

    /// <summary>
    /// Play life gauge effect on heal
    /// </summary>
    /// <param name="healRatio">Normalized heal ratio to the life max</param>
    /// <param name="lifeRatio">Normalized life ratio after healing to the life max</param>
    public void OnHeal(float healRatio, float lifeRatio)
    {
        effectOnUpdate = true;
        healEffect.PlayEffect(healRatio * 0.5f, lifeRatio);
    }

    public void OnNoEffectHeal(float heal, float prevLife)
    {
        float life = heal + prevLife;

        effectOnUpdate = true;
        bool isHealVisible = (int)life > (int)prevLife;
        if (isHealVisible) smallHealSound.PlayEx();
    }

    public void OnLifeMax()
    {
        lifeMaxSound.PlayEx();
    }

    public void OnDamage(float damageRatio)
    {
        if (damageRatio < 0.000001f)
        {
            effectOnUpdate = false;
            return;
        }

        healEffect.KillEffect();

        effectOnUpdate = true;

        shakeTween?.Kill();
        shakeTween = DamageShake(damageRatio);
    }

    public static string GetDisplayHP(float life, float lifeMax)
    {
        int hp = life < 1.0f ? (life > 0.0f ? 1 : 0) : (int)life;
        return hp + " / " + (int)lifeMax;
    }

    public void UpdateLifeText(float life, float lifeMax)
    {
        lifeText.text = GetDisplayHP(life, lifeMax);
    }

    private Tween DamageShake(float damageRatio)
    {
        if (damageRatio <= 0.0f) return null;
        return rectTransform.DOShakeAnchorPos(2 * damageRatio, 100 * damageRatio, (int)(300 * damageRatio)).Play();
    }
}
