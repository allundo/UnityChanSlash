using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField] private GameObject ball = default;
    [SerializeField] private RectTransform rtFrame = default;
    [SerializeField] private Sprite frameEmpty = default;
    [SerializeField] private ParticleSystem vfxRain = default;
    [SerializeField] private BlinkTMP nowLoading = default;
    private Image imageFrame;

    private RectTransform rtBackGround;

    private UITween ballTween;

    void Awake()
    {
        rtBackGround = GetComponent<RectTransform>();
        imageFrame = rtFrame.GetComponent<Image>();
        nowLoading.Inactivate();

        ballTween = new UITween(ball);
    }

    public void Inactivate()
    {
        gameObject.SetActive(false);
    }

    public Tween LogoTween()
    {
        return
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    ballTween.SetPos(new Vector2(-1080.0f, 0f));
                    imageFrame.fillAmount = 0f;
                })
                .Append(ballTween.MoveBack(1f).SetEase(Ease.InQuad))
                .Join(ballTween.Rotate(-360f, 1f).SetEase(Ease.InQuad))
                .Append(ballTween.MoveX(10f, 0.2f).SetEase(Ease.OutQuad))
                .Join(ballTween.Rotate(-10f, 0.2f, false).SetEase(Ease.OutQuad))
                .Append(ballTween.MoveBack(0.2f).SetEase(Ease.InQuad))
                .Join(ballTween.Rotate(0f, 0.2f, false).SetEase(Ease.InQuad))
                .AppendInterval(0.4f)
                .Append(ballTween.MoveY(-50f, 0.1f).SetEase(Ease.OutQuad))
                .Append(ballTween.MoveBack(0.1f).SetEase(Ease.InQuad))
                .AppendInterval(0.1f)
                .Append(DOTween.To(() => imageFrame.fillAmount, fill => imageFrame.fillAmount = fill, 1f, 0.15f).SetEase(Ease.Linear))
                .AppendInterval(0.15f)
                .AppendCallback(() => nowLoading.Activate());
    }

    public void ToTitle()
    {
        imageFrame.sprite = frameEmpty;
        vfxRain?.Stop();
        nowLoading.Inactivate();

        DOVirtual.DelayedCall(0.1f, () => vfxRain?.Pause()).Play();
        DOVirtual.DelayedCall(0.5f, () => vfxRain?.Clear()).Play();

        rtBackGround
            .DOAnchorPos(new Vector2(2820f, 0), 1.8f)
            .SetDelay(0.2f)
            .OnComplete(() => rtBackGround.transform.gameObject.SetActive(false))
            .Play();
    }
}
