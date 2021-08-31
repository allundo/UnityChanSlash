using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform[] titleChildren = default;

    private UITween baseTween;
    private List<UITween> childTweens = new List<UITween>();

    void Awake()
    {
        baseTween = new UITween(gameObject);
        childTweens = titleChildren.Select(rt => new UITween(rt.gameObject)).ToList();
    }

    public void TitleTween()
    {
        DOTween.Sequence()
            .AppendCallback(() => baseTween.SetPosX(-2820f))
            .AppendInterval(0.4f)
            .Append(baseTween.MoveBack(0.6f).SetEase(Ease.Linear))
            .Append(Overrun(480f, 0.5f))
            .Join(SizeTweenAll(0.5f, 0.5f))
            .Play();
    }

    private Tween Overrun(float overrun, float duration)
        => DOTween.Sequence()
            .Append(baseTween.MoveX(overrun, duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(baseTween.MoveBack(duration * 0.5f).SetEase(Ease.InQuad));

    private Tween SizeTweenAll(float scale, float duration)
    {
        Sequence seq = DOTween.Sequence();

        childTweens.ForEach(tween => seq.Join(SizeTween(tween, scale, duration)));

        return seq;
    }

    private Tween SizeTween(UITween tween, float scale, float duration)
        => DOTween.Sequence()
            .Append(tween.Resize(scale, duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(tween.Resize(1f, duration * 0.5f).SetEase(Ease.InQuad));

    public Tween CameraOutTween() => baseTween.MoveY(1280f, 0.2f);
}
