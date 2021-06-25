using UnityEngine;
using DG.Tweening;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform[] rtTitles = default;

    private Vector2[] defaultSizes;
    private RectTransform rt;

    private Vector2 defaultPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        defaultPos = rt.anchoredPosition;

        defaultSizes = new Vector2[rtTitles.Length];

        for (int i = 0; i < rtTitles.Length; i++)
        {
            defaultSizes[i] = rtTitles[i].sizeDelta;
        }
    }

    public void TitleTween()
    {
        rt.anchoredPosition = new Vector2(-2820f, defaultPos.y);

        DOTween.Sequence()
            .AppendInterval(0.5f)
            .Append(rt.DOAnchorPos(new Vector2(0f, defaultPos.y), 0.5f).SetEase(Ease.Linear))
            .Append(Overrun(480f, 0.5f))
            .Join(SizeTweenAll(0.5f, 0.5f))
            .Play();
    }

    private Tween Overrun(float overrun, float duration)
    {
        return DOTween.Sequence()
            .Append(rt.DOAnchorPos(new Vector2(overrun, defaultPos.y), duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(new Vector2(0f, defaultPos.y), duration * 0.5f).SetEase(Ease.InQuad));

    }

    private Tween SizeTweenAll(float scale, float duration)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < rtTitles.Length; i++)
        {
            seq.Join(SizeTween(rtTitles[i], defaultSizes[i], scale, duration));
        }

        return seq;
    }

    private Tween SizeTween(RectTransform rt, Vector2 defaultSize, float scale, float duration)
    {
        return DOTween.Sequence()
            .Append(rt.DOSizeDelta(defaultSize * scale, duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(rt.DOSizeDelta(defaultSize, duration * 0.5f).SetEase(Ease.InQuad));
    }
}
