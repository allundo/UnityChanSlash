using UnityEngine;
using DG.Tweening;
public class RankInMessage : MonoBehaviour
{
    [SerializeField] private GameObject rankInText = default;
    [SerializeField] private ParticleSystem textEffect = default;
    [SerializeField] private ParticleSystem shootEffect = default;
    private UITween uiTween;
    private FadeTween bgFade;
    private RectTransform rectTransform;

    private float moveX;

    void Awake()
    {
        uiTween = new UITween(rankInText);
        uiTween.SetPosX(Screen.width);
        rectTransform = GetComponent<RectTransform>();
        bgFade = new FadeTween(gameObject, 0.5f);
        bgFade.SetAlpha(0f);
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        rectTransform.sizeDelta = new Vector2(Screen.width, rectTransform.sizeDelta.y);
        moveX = -(Screen.width + uiTween.defaultSize.x) * 0.5f;
        uiTween.SetPos(new Vector2(-moveX, 0f), true);
    }

    public Tween RankInTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => uiTween.ResetPos())
            .AppendCallback(() => textEffect.StopAndClear())
            .AppendCallback(() => textEffect.PlayEx())
            .AppendCallback(() => shootEffect.PlayEx())
            .Append(uiTween.MoveX(moveX, 1.5f).SetEase(Ease.OutExpo))
            .Join(bgFade.In(1.5f, 0, null, null, false))
            .AppendInterval(0.25f)
            .Append(uiTween.MoveX(moveX, 1f).SetEase(Ease.InExpo))
            .Join(bgFade.Out(1f, 0, null, null, false).SetEase(Ease.InQuad));
    }
}
