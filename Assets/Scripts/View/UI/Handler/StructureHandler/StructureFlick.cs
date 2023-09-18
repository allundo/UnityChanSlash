using UniRx;

public class StructureFlick : FlickInteraction
{
    protected IReactiveProperty<bool> isHandOn = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsHandOn => isHandOn;

    protected override void SetFlicks()
    {
        up = FlickUp.New(this);
        down = StructureFlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    protected override void Clear()
    {
        base.Clear();
        isHandOn.Value = false;
    }

    protected class StructureFlickDown : FlickDown
    {
        private StructureFlickDown(StructureFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f);
        }

        public static StructureFlickDown New(StructureFlick flick)
        {
            return IsValid(flick) ? new StructureFlickDown(flick) : null;
        }
    }
}
