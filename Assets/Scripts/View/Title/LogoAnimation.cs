using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform rtBall = default;
    [SerializeField] private RectTransform rtFrame = default;
    [SerializeField] private Sprite frameEmpty = default;
    [SerializeField] private ParticleSystem vfxRain = default;
    [SerializeField] private BlinkTMP nowLoading = default;
    private Image imageFrame;

    private RectTransform rtButton;
    private RectTransform rtBackGround;

    void Awake()
    {
        rtBackGround = GetComponent<RectTransform>();
        imageFrame = rtFrame.GetComponent<Image>();
        nowLoading.Inactivate();
    }

    public void Inactivate()
    {
        gameObject.SetActive(false);
    }

    public void LogoTween(TweenCallback OnComplete = null)
    {
        OnComplete = OnComplete ?? (() => { });

        imageFrame.fillAmount = 0f;
        rtBall.anchoredPosition = new Vector2(-1080.0f, 0f);

        DOTween.Sequence()
            .Append(rtBall.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.InQuad))
            .Join(rtBall.DORotate(new Vector3(0f, 0f, -360f), 1f, RotateMode.FastBeyond360).SetEase(Ease.InQuad))
            .Append(rtBall.DOAnchorPos(new Vector2(10f, 0f), 0.2f).SetEase(Ease.OutQuad))
            .Join(rtBall.DORotate(new Vector3(0f, 0f, -10f), 0.2f, RotateMode.Fast).SetEase(Ease.OutQuad))
            .Append(rtBall.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.InQuad))
            .Join(rtBall.DORotate(Vector3.zero, 0.2f, RotateMode.Fast).SetEase(Ease.InQuad))
            .AppendInterval(0.4f)
            .Append(rtBall.DOAnchorPos(new Vector2(0f, -50f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rtBall.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .AppendInterval(0.1f)
            .Append(DOTween.To(() => imageFrame.fillAmount, fill => imageFrame.fillAmount = fill, 1f, 0.15f).SetEase(Ease.Linear))
            .AppendInterval(0.15f)
            .AppendCallback(() => nowLoading.Activate())
            .OnComplete(OnComplete)
            .Play();
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
