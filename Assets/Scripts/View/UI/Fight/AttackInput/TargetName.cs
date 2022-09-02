using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Image))]
public class TargetName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmName = default;

    private Image pinImage;

    private bool isActive = false;

    private Tween activateTween;
    private Tween inactivateTween;

    private FadeTween fade;
    private TextTween nameTextTween;
    private FadeTween nameFadeTween;

    void Awake()
    {
        pinImage = GetComponent<Image>();
        pinImage.fillAmount = 0f;

        fade = new FadeTween(pinImage, 0.8f);
        nameTextTween = new TextTween(tmName.gameObject);
        nameFadeTween = new FadeTween(tmName.gameObject, 0.95f);

        fade.SetAlpha(0f);
        nameFadeTween.SetAlpha(0f);

        gameObject.SetActive(false);
    }

    public void Activate(string name)
    {
        gameObject.SetActive(true);

        activateTween?.Kill();
        inactivateTween?.Kill();

        pinImage.fillAmount = 0f;
        nameTextTween.ResetPos();

        tmName.text = name;

        activateTween = DOTween.Sequence()
            .Join(fade.In(0.1f, 0f, null, null, false))
            .Join(DOVirtual.Float(0f, 1f, 0.3f, value => pinImage.fillAmount = value))
            .Join(nameTextTween.MoveX(40f, 0.25f).SetDelay(0.05f))
            .Join(nameFadeTween.In(0.25f, 0.05f, null, null, false))
            .AppendInterval(3f)
            .AppendCallback(Inactivate)
            .Play();

        isActive = true;
    }

    public void Inactivate()
    {
        if (!isActive) return;

        activateTween?.Kill();
        inactivateTween?.Kill();

        activateTween = DOTween.Sequence()
            .Join(fade.Out(0.6f, 0f, null, () => gameObject.SetActive(false), true))
            .Join(nameFadeTween.Out(0.6f, 0f, null, null, true))
            .Play();

        isActive = false;
    }
}