using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;
using System.Linq;

public class FlickInteraction : FadeActivate
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
    private FlickDirection defaultDir = null;
    private FlickDirection[] flickDirections;

    protected Image image;
    protected UITween ui;
    protected Tween fadeInactive = null;
    protected Tween fadeOutActive = null;

    public IObservable<Unit> UpSubject => up.FlickSubject;
    public IObservable<Unit> DownSubject => down.FlickSubject;
    public IObservable<Unit> RightSubject => right.FlickSubject;
    public IObservable<Unit> LeftSubject => left.FlickSubject;

    public IObservable<Unit> FlickSubject { get; protected set; }
    public IObservable<Unit> ReleaseSubject { get; protected set; }

    public IReadOnlyReactiveProperty<float> DragUp => up.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragDown => down.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragRight => right.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragLeft => left.DragRatioRP;

    public IObservable<FlickDirection> Drag { get; protected set; }

    protected override void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image, maxAlpha);
        ui = new UITween(gameObject);

        SetFlicks();
        flickDirections = new FlickDirection[] { up, down, right, left };

        // Set image.sprite as default icon of the handling UI
        defaultDir = flickDirections.Where(x => x?.sprite == image.sprite).FirstOrDefault();

        FlickSubject = Merge(flick => flick.FlickSubject);
        ReleaseSubject = Merge(flick => flick.ReleaseSubject);
        Drag = Merge(flick => flick.Drag);
    }

    public Tween FadeOutActive(float duration = 0.2f)
    {
        fadeOutActive = fade.ToAlpha(0, duration)
            .OnPlay(() => isActive = false)
            .OnComplete(() =>
            {
                InitImage();
                Clear();
                FadeIn(0.1f).Play();
            });

        return DOTween.Sequence()
            .AppendCallback(() => { if (isActive) fadeOutActive?.Play(); });
    }

    private IObservable<T> Merge<T>(Func<FlickDirection, IObservable<T>> observable)
        => Observable.Merge(flickDirections.Where(x => x != null).Select(observable));

    protected virtual void SetFlicks()
    {
        up = FlickUp.New(this);
        down = FlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    public void Release(Vector2 dragVector, float duration = 0.2f)
    {
        currentDir?.Release(dragVector, duration);
    }

    public void Cancel(float duration = 0.2f)
    {
        ui.MoveBack(duration).Play();
        ui.Resize(0.5f, duration).Play();
        FadeOutActive(duration).Play();
    }

    public override void Activate()
    {
        defaultDir.SetSprite();
        ui.ResetSize();
        ui.ResetPos();
        base.Activate();
    }

    public override void Inactivate()
    {
        Clear();
        base.Inactivate();
    }

    public override Tween FadeIn(float duration = 0.2f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        onPlay = onPlay ?? (() => { });

        return base.FadeIn(duration,
            () =>
            {
                InitImage();
                onPlay();
            },
            onComplete,
            isContinuous
        );

    }
    public override Tween FadeOut(float duration = 0.2f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return base.FadeOut(duration,
            () =>
            {
                fadeOutActive?.Kill();
                onPlay();
            },
            () =>
            {
                Clear();
                onComplete();
            },
            isContinuous
        );
    }

    public void UpdateImage(Vector2 dragVector)
    {
        currentDir = GetDirection(dragVector);
        currentDir?.UpdateImage(dragVector);
    }

    private FlickDirection GetDirection(Vector2 dragVector)
    {
        if (dragVector == Vector2.zero && defaultDir != null) return defaultDir;

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

    protected void InitImage()
    {
        defaultDir.SetSprite();
        ui.ResetSize();
        ui.ResetPos();
    }

    public abstract class FlickDirection
    {
        protected FlickInteraction flick;
        protected FadeTween fade;
        protected UITween ui;
        public Sprite sprite { get; protected set; }
        protected float limit;

        protected ReactiveProperty<float> dragRatio = new ReactiveProperty<float>(0.0f);
        public IReadOnlyReactiveProperty<float> DragRatioRP => dragRatio;
        public float Ratio => dragRatio.Value;

        public IObservable<FlickDirection> Drag;

        protected ISubject<Unit> flickSubject = new Subject<Unit>();
        public IObservable<Unit> FlickSubject => flickSubject;

        protected ISubject<Unit> releaseSubject = new Subject<Unit>();
        public IObservable<Unit> ReleaseSubject => releaseSubject;

        protected FlickDirection(FlickInteraction flick, Sprite sprite, float limit)
        {
            this.flick = flick;
            this.fade = flick.fade;
            this.ui = flick.ui;
            this.sprite = sprite;
            this.limit = limit;

            Drag = DragRatioRP.Select(_ => this);
        }

        protected virtual Vector2 Destination => Vector2.zero;

        /// <summary>
        /// Returns drag directional factor limited by 'limit' property.
        /// </summary>
        protected virtual Vector2 LimitedVec(Vector2 dragVector) => Vector2.zero;

        private void FlickOnNext()
        {
            flickSubject.OnNext(Unit.Default);
        }

        private void ReleaseOnNext()
        {
            releaseSubject.OnNext(Unit.Default);
        }

        public void SetSprite() => fade.SetSprite(sprite);

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

            float degree = 1f - dragRatio.Value * 0.5f;
            ui.ResetSize(0.5f + degree);
            fade.SetAlpha(degree);
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
                .Join(flick.FadeOutActive(duration))
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