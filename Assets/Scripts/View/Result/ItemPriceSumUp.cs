using DG.Tweening;
using TMPro;

public class ItemPriceSumUp : ResultAnimation
{
    protected override void Awake()
    {
        LoadLabelTxt();

        valueTxt = labelTxt;

        valueUI = new TextTween(valueTxt.gameObject);
        valueFade = new FadeTween(valueTxt.gameObject);

        valueFade.SetAlpha(0f);
    }
    protected override string ValueFormat(ulong value) => $"アイテム総資産: ￥{value:#,0}";

    public override Tween Centering(float vecX, float duration)
    {
        return valueUI.MoveX(vecX, duration).OnPlay(() => valueTxt.alignment = TextAlignmentOptions.Center);
    }
}
