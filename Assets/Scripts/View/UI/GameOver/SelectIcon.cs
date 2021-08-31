using UnityEngine;
using DG.Tweening;

public class SelectIcon : MonoBehaviour
{
    private UITween uiTween;

    private Tween selectTween = null;

    void Awake()
    {
        uiTween = new UITween(gameObject, true);
    }

    public Tween SelectTween(Vector2 iconPos)
    {

        uiTween.SetPos(iconPos, true);

        selectTween?.Kill();

        selectTween =
            DOTween.Sequence()
                .Append(uiTween.MoveY(-30f, 0.01f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(new Vector2(1.5f, 0.5f), 0.01f))
                .Append(uiTween.MoveBack(0.05f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(1f, 0.05f))
                .Append(uiTween.MoveY(50f, 0.3f).SetEase(Ease.OutQuad))
                .SetLoops(-1, LoopType.Yoyo)
                .AsReusable(gameObject)
                .Play();

        return selectTween;
    }
}
