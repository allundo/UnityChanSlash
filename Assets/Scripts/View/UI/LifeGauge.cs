using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LifeGauge : MonoBehaviour
{
    [SerializeField] private MobStatus status = default;
    [SerializeField] private Image GreenGauge = default;
    [SerializeField] private Image RedGauge = default;
    // [SerializeField] private TextMeshPro life;
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
        GreenGauge.color = ratio[5];
        status.Life.Subscribe(life => OnLifeChange(life / status.LifeMax));
    }

    private void OnLifeChange(float lifeRatio)
    {
        float damageRatio = GreenGauge.fillAmount - lifeRatio;

        GreenGauge.fillAmount = lifeRatio;

        for (float compare = 5.0f; compare >= 0.0f; compare -= 1.0f)
        {
            if (lifeRatio > compare / 6.0f)
            {
                GreenGauge.color = new Color(1, 1, 1);
                greenGaugeTween?.Kill();
                greenGaugeTween = GreenGauge.DOColor(ratio[(int)compare], 0.5f).Play();
                break;
            }
        }

        redGaugeTween?.Kill();
        redGaugeTween = GetRedGaugeTween(RedGauge.fillAmount, lifeRatio).Play();

        shakeTween?.Kill();
        shakeTween = GetShake(100 * damageRatio, 2.0f * damageRatio).Play();
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

    private Tween GetShake(float strength, float duration = 1f)
    {
        return rectTransform.DOShakeAnchorPos(duration, strength, 30);
    }

}
