using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

public class FlickInteraction : MonoBehaviour
{
    [SerializeField] protected Sprite upSprite = null;
    [SerializeField] protected Sprite downSprite = null;
    [SerializeField] protected Sprite leftSprite = null;
    [SerializeField] protected Sprite rightSprite = null;

    [SerializeField] protected float upLimit = 0.0f;
    [SerializeField] protected float downLimit = 0.0f;
    [SerializeField] protected float rightLimit = 0.0f;
    [SerializeField] protected float leftLimit = 0.0f;

    [SerializeField] protected float maxAlpha = 1.0f;

    protected FlickDirection up = null;
    protected FlickDirection down = null;
    protected FlickDirection right = null;
    protected FlickDirection left = null;

    private FlickDirection currentDir = null;

    protected FadeTween fade;
    protected UITween ui;

    public ISubject<Unit> UpSubject => up.FlickSubject;
    public ISubject<Unit> DownSubject => down.FlickSubject;
    public ISubject<Unit> RightSubject => right.FlickSubject;
    public ISubject<Unit> LeftSubject => left.FlickSubject;

    public IObservable<Unit> ReleaseSubject { get; protected set; } = new Subject<Unit>();

    public IReadOnlyReactiveProperty<float> DragUp => up.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragDown => down.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragRight => right.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragLeft => left.DragRatioRP;

    void Awake()
    {
        fade = new FadeTween(gameObject, maxAlpha);
        ui = new UITween(gameObject);

        SetFlicks();

        foreach (var flick in new FlickDirection[] { up, down, right, left })
        {
            if (flick != null) ReleaseSubject = ReleaseSubject.Merge(flick.ReleaseSubject);
        }
    }

    protected virtual void SetFlicks()
    {
        up = FlickUp.New(this);
        down = FlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    private Tween GetFadeOutToInactive(float duration = 0.2f)
        => fade.Out(duration, 0, Clear, () => gameObject.SetActive(false));

    public void Release(Vector2 dragVector, float duration = 0.2f)
    {
        currentDir?.Release(dragVector, duration);
    }

    public void Cancel(float duration = 0.2f)
    {
        ui.MoveBack(duration).Play();
        ui.Resize(0.5f, duration).Play();
        GetFadeOutToInactive(duration).Play();
    }

    private void Activate(Vector2 dragVector)
    {
        if (currentDir != null) return;

        ui.ResetSize();
        gameObject.SetActive(true);
    }

    public virtual void Inactivate(float duration = 0.2f)
    {
        if (currentDir == null) return;

        GetFadeOutToInactive(duration).Play();
    }

    public void UpdateImage(Vector2 dragVector)
    {
        Activate(dragVector);

        currentDir = GetDirection(dragVector);
        currentDir?.UpdateImage(dragVector);
    }

    private FlickDirection GetDirection(Vector2 dragVector)
    {
        FlickDirection dir;
        if (Mathf.Abs(dragVector.x) >= Mathf.Abs(dragVector.y))
        {
            dir = dragVector.x > 0 ? right : left;
            dir = dir ?? (dragVector.y > 0 ? up : down) ?? right ?? left;
        }
        else
        {
            dir = dragVector.y > 0 ? up : down;
            dir = dir ?? (dragVector.x > 0 ? right : left) ?? up ?? down;
        }

        return dir;
    }

    protected virtual void Clear()
    {
        currentDir = null;
    }

    protected abstract class FlickDirection
    {
        protected FlickInteraction flick;
        protected FadeTween fade;
        protected UITween ui;
        public Sprite sprite { get; protected set; }
        protected float limit;

        protected ReactiveProperty<float> dragRatio = new ReactiveProperty<float>(0.0f);
        public IReadOnlyReactiveProperty<float> DragRatioRP => dragRatio;
        public float Ratio => dragRatio.Value;

        public ISubject<Unit> FlickSubject { get; protected set; } = new Subject<Unit>();
        public ISubject<Unit> ReleaseSubject { get; protected set; } = new Subject<Unit>();

        protected FlickDirection(FlickInteraction flick, Sprite sprite, float limit)
        {
            this.flick = flick;
            this.fade = flick.fade;
            this.ui = flick.ui;
            this.sprite = sprite;
            this.limit = limit;
        }

        protected virtual Vector2 Destination => Vector2.zero;

        /// <summary>
        /// Returns drag directional factor limited by 'limit' property.
        /// </summary>
        protected virtual Vector2 LimitedVec(Vector2 dragVector) => Vector2.zero;

        private void FlickOnNext()
        {
            FlickSubject.OnNext(Unit.Default);
        }

        private void ReleaseOnNext()
        {
            ReleaseSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// Change sprite, move by drag directional factor and set alpha according to drag directional ratio.
        /// </summary>
        /// <param name="dragVector"></param>
        public void UpdateImage(Vector2 dragVector)
        {
            fade.SetSprite(sprite);

            Vector2 limitedVec = LimitedVec(dragVector);

            dragRatio.SetValueAndForceNotify(DragRatio(limitedVec.magnitude));

            ui.SetPosOffset(limitedVec);
            fade.SetAlpha(dragRatio.Value);
        }

        /// <summary>
        /// Returns drag directional ratio to 'limit' property.
        /// </summary>
        /// <param name="abs">Absolute value of drag directional factor</param>
        private float DragRatio(float abs) => abs / limit;
        private float DragRatio(Vector2 dragVector) => DragRatio(LimitedVec(dragVector).magnitude);

        public void Release(Vector2 dragVector, float duration = 0.2f)
        {
            ReleaseOnNext();

            if (DragRatio(dragVector) < 0.5f)
            {
                flick.Cancel(duration);
                return;
            }

            DOTween.Sequence()
                .Append(ui.MoveOffset(Destination, duration))
                .Join(fade.In(duration))
                .Append(ui.Resize(1.5f, duration))
                .Join(flick.GetFadeOutToInactive(duration))
                .Play();

            FlickOnNext();
        }
    }

    protected class FlickUp : FlickDirection
    {
        protected FlickUp(FlickInteraction flick) : base(flick, flick.upSprite, flick.upLimit) { }

        public static FlickUp New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickUp(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.upLimit > 0.0f && flick.upSprite != null;

        protected override Vector2 Destination => new Vector2(0, limit * 1.5f);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(0, Mathf.Clamp(dragVector.y, 0, limit));
    }

    protected class FlickDown : FlickDirection
    {
        protected FlickDown(FlickInteraction flick) : base(flick, flick.downSprite, flick.downLimit) { }

        public static FlickDown New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickDown(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.downLimit > 0.0f && flick.downSprite != null;

        protected override Vector2 Destination => new Vector2(0, -limit * 1.5f);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(0, Mathf.Clamp(dragVector.y, -limit, 0));
    }

    protected class FlickRight : FlickDirection
    {
        protected FlickRight(FlickInteraction flick) : base(flick, flick.rightSprite, flick.rightLimit) { }

        public static FlickRight New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickRight(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.rightLimit > 0.0f && flick.rightSprite != null;

        protected override Vector2 Destination => new Vector2(limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Clamp(dragVector.x, 0, limit), 0);
    }

    protected class FlickLeft : FlickDirection
    {
        protected FlickLeft(FlickInteraction flick) : base(flick, flick.leftSprite, flick.leftLimit) { }

        public static FlickLeft New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickLeft(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.leftLimit > 0.0f && flick.leftSprite != null;

        protected override Vector2 Destination => new Vector2(-limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Clamp(dragVector.x, -limit, 0), 0);
    }
}