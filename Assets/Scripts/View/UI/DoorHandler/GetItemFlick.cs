using UniRx;

public class GetItemFlick : FlickInteraction
{
    protected IReactiveProperty<bool> isHandOn = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsHandOn => isHandOn;

    protected override void SetFlicks()
    {
        up = FlickUp.New(this);
        down = GetItemFlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    protected override void Clear()
    {
        base.Clear();
        isHandOn.Value = false;
    }

    protected class GetItemFlickDown : FlickDown
    {
        private GetItemFlickDown(GetItemFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f);
        }

        public static GetItemFlickDown New(GetItemFlick flick)
            => IsValid(flick) ? new GetItemFlickDown(flick) : null;
    }
}
