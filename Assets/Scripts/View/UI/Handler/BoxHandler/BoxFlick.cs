using UniRx;

public class BoxFlick : FlickInteraction
{
    protected IReactiveProperty<bool> isHandOn = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsHandOn => isHandOn;

    protected override void SetFlicks()
    {
        up = BoxFlickUp.New(this);
        down = BoxFlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    protected override void Clear()
    {
        base.Clear();
        isHandOn.Value = false;
    }

    protected class BoxFlickUp : FlickUp
    {
        private BoxFlickUp(BoxFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f);
        }

        public static BoxFlickUp New(BoxFlick flick)
        {
            return IsValid(flick) ? new BoxFlickUp(flick) : null;
        }
    }

    protected class BoxFlickDown : FlickDown
    {
        private BoxFlickDown(BoxFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f);
        }

        public static BoxFlickDown New(BoxFlick flick)
        {
            return IsValid(flick) ? new BoxFlickDown(flick) : null;
        }
    }
}