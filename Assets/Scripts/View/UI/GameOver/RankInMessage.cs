using UnityEngine;
using DG.Tweening;
public class RankInMessage : MonoBehaviour
{
    [SerializeField] private GameObject rankInText = default;
    [SerializeField] private ParticleSystem textEffect = default;
    [SerializeField] private ParticleSystem shootEffect = default;
    private UITween uiTween;
    private FadeTween bgFade;

    void Awake()
    {
        uiTween = new UITween(rankInText);
        uiTween.SetPosX(Screen.width);
        bgFade = new FadeTween(gameObject, 0.5f);
        bgFade.SetAlpha(0f);
    }

    public Tween RankInTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => uiTween.SetPosX(Screen.width))
            .AppendCallback(() => textEffect.PlayEx())
            .AppendCallback(() => shootEffect.PlayEx())
            .Append(uiTween.MoveX(-Screen.width, 1.5f).SetEase(Ease.OutExpo))
            .Join(bgFade.In(1.5f, 0, null, null, false))
            .AppendInterval(0.25f)
            .Append(uiTween.MoveX(-Screen.width, 1f).SetEase(Ease.InExpo))
            .Join(bgFade.Out(1f, 0, null, null, false).SetEase(Ease.InQuad))
            .AppendCallback(() => textEffect.StopEmitting());
    }
}