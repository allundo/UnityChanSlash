using UniRx;

public class DoorFlick : FlickInteraction
{
    protected IReactiveProperty<bool> isHandOn = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsHandOn => isHandOn;

    protected override void SetFlicks()
    {
        up = FlickUp.New(this);
        down = FlickDown.New(this);
        right = DoorFlickRight.New(this);
        left = DoorFlickLeft.New(this);
    }

    protected override void Clear()
    {
        base.Clear();
        isHandOn.Value = false;
    }

    protected class DoorFlickRight : FlickRight
    {
        private DoorFlickRight(DoorFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f).AddTo(flick);
        }

        public static DoorFlickRight New(DoorFlick flick)
        {
            return IsValid(flick) ? new DoorFlickRight(flick) : null;
        }
    }

    protected class DoorFlickLeft : FlickLeft
    {
        private DoorFlickLeft(DoorFlick flick) : base(flick)
        {
            DragRatioRP.Subscribe(ratio => flick.isHandOn.Value = ratio > 0.5f).AddTo(flick);
        }

        public static DoorFlickLeft New(DoorFlick flick)
        {
            return IsValid(flick) ? new DoorFlickLeft(flick) : null;
        }
    }
}