using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TitleDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTxt = default;
    [SerializeField] private TextMeshProUGUI labelTxt = default;
    [SerializeField] private Image strokeImage = default;
    [SerializeField] private AudioSource titleShowSnd = default;
    [SerializeField] private AudioSource titleStrokeSnd = default;

    private Image bgImage;
    private RectTransform bgRT;
    private UITween bgUI;
    private FadeTween bgFade;
    private TextTween titleUI;
    private FadeTween titleFade;
    private TextTween labelUI;
    private FadeTween labelFade;
    private FadeTween strokeFade;
    private UITween strokeUI;

    void Awake()
    {
        bgImage = GetComponent<Image>();
        bgRT = GetComponent<RectTransform>();

        bgRT.sizeDelta = new Vector2(Screen.width, 720f);

        bgUI = new UITween(bgImage.gameObject);
        bgFade = new FadeTween(bgImage, 0.75f);
        titleUI = new TextTween(titleTxt.gameObject);
        titleFade = new FadeTween(titleTxt.gameObject);
        labelUI = new TextTween(labelTxt.gameObject);
        labelFade = new FadeTween(labelTxt.gameObject);
        strokeUI = new UITween(strokeImage.gameObject);
        strokeFade = new FadeTween(strokeImage, 0.75f);

        labelFade.SetAlpha(0f);
        titleFade.SetAlpha(0f);
        strokeImage.fillAmount = 0f;
        bgUI.ResetSizeY(0f);
        bgFade.SetAlpha(0f);
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    public Tween DisplayTween(string title, TweenCallback wagesTweenPlayer, float duration = 6f)
    {
        float endUp = duration * 0.15f;

        return DOTween.Sequence()
            .AppendCallback(() => gameObject.SetActive(true))
            .Join(bgUI.ResizeY(1f, duration * 0.1f))
            .Join(bgFade.In(duration * 0.1f, 0, null, null, false))
            .InsertCallback(duration * 0.06f, () => labelFade.In(duration * 0.2f, 0, null, null, false).Play())
            .InsertCallback(duration * 0.15f, () => StrokeFill(duration * 0.15f).Play())
            .AppendCallback(() =>
            {
                titleTxt.text = title;
                titleUI.ResetSize(2f);
            })
            .Join(titleFade.In(duration * 0.3f, 0, null, null, false).SetEase(Ease.InCubic))
            .Join(titleUI.Resize(1.05f, duration * 0.3f).SetEase(Ease.InCubic))
            .AppendCallback(wagesTweenPlayer)
            .AppendCallback(() => titleShowSnd.PlayEx())
            .Append(titleUI.PunchY(20f, duration * 0.1f, 20))
            .Append(titleUI.Resize(1f, duration * 0.1f))
            .AppendInterval(duration * 0.2f)
            .Append(titleUI.MoveX(Screen.width, endUp))
            .Join(titleUI.Resize(0.5f, endUp))
            .Join(labelUI.MoveX(-Screen.width, endUp))
            .Join(labelFade.Out(endUp, 0, null, null, false))
            .Join(titleFade.Out(endUp, 0, null, null, false))
            .Join(bgUI.ResizeY(0f, endUp).SetEase(Ease.Linear))
            .Join(bgFade.Out(endUp, 0, null, null, false))
            .Join(strokeUI.Resize(new Vector2(1.2f, 0f), endUp))
            .Join(strokeFade.Out(endUp, 0, null, null, false))
            .AppendCallback(() => gameObject.SetActive(false));
    }

    public Tween FadeBG(float duration)
    {
        return DOTween.Sequence()
            .Join(bgUI.ResizeY(1f, duration * 0.5f))
            .Join(bgFade.In(duration * 0.5f))
            .Append(bgFade.Out(duration * 0.5f));
    }

    public Tween FadeOutBG(float duration)
    {
        bgUI.ResetSize();
        bgFade.SetAlpha(1f);
        return bgFade.Out(duration).Play();
    }

    public Tween FadeTitle(float duration)
    {
        return DOTween.Sequence()
            .Append(titleFade.In(duration * 0.5f))
            .Append(titleFade.Out(duration * 0.5f));
    }

    public Tween FadeOutTitle(float duration)
    {
        titleFade.SetAlpha(1f);
        return titleFade.Out(duration).Play();
    }

    public Tween FadeLabel(float duration)
    {
        return DOTween.Sequence()
            .Append(labelFade.In(duration * 0.5f))
            .Append(labelFade.Out(duration * 0.5f));
    }

    public Tween FadeOutLabel(float duration)
    {
        labelFade.SetAlpha(1f);
        return labelFade.Out(duration).Play();
    }

    public Tween FadeStroke(float duration)
    {
        return DOTween.Sequence()
            .Join(StrokeFill(duration * 0.5f))
            .Join(strokeFade.In(duration * 0.5f))
            .Append(strokeFade.Out(duration * 0.5f));
    }

    public Tween FadeOutStroke(float duration)
    {
        strokeImage.fillAmount = 1f;
        strokeFade.SetAlpha(1f);
        return strokeFade.Out(duration).Play();
    }

    private Tween StrokeFill(float duration)
    {
        return DOVirtual.Float(0f, 1f, duration, value => strokeImage.fillAmount = value).OnPlay(() => titleStrokeSnd.PlayEx());
    }
}
