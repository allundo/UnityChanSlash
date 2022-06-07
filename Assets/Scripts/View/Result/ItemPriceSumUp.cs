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
}

