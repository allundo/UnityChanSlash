using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StartButton : MonoBehaviour
{
    private RectTransform rt;
    public Button startButton { get; private set; }
    private Image image;
    private Vector2 defaultSize;

    void Awake()
    {
        startButton = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        defaultSize = rt.sizeDelta;

        startButton.interactable = false;
    }

    public void LogoTween()
    {
        rt.anchoredPosition = new Vector2(0f, 1920.0f);

        DOTween.Sequence()
            .AppendInterval(0.8f)
            .Append(rt.DOAnchorPos(Vector2.zero, 1.0f).SetEase(Ease.InQuad))
            .Append(rt.DOAnchorPos(new Vector2(0f, -100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rt.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .Join(rt.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Play();
    }

    public void ToTitle()
    {
        DOTween.Sequence()
            .Append(rt.DOAnchorPos(new Vector2(0f, -100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rt.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .Join(rt.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Append(rt.DOJump(new Vector3(-240f, 0f, 0f), 1000f, 1, 0.6f).SetRelative())
            .Join(rt.DOSizeDelta(defaultSize * 0.5f, 0.1f).SetEase(Ease.Linear))
            .Join(rt.DORotate(new Vector3(0f, 0f, 720f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .AppendCallback(() => startButton.interactable = true)
            .Play();
    }
}
