using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using System;
using System.Linq;

public class FlickInteraction : FadeEnable, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] protected Sprite upSprite = null;
    [SerializeField] protected Sprite downSprite = null;
    [SerializeField] protected Sprite rightSprite = null;
    [SerializeField] protected Sprite leftSprite = null;

    [SerializeField] private string upText = null;
    [SerializeField] private string downText = null;
    [SerializeField] private string rightText = null;
    [SerializeField] private string leftText = null;

    [SerializeField] protected float upLimit = 0.0f;
    [SerializeField] protected float downLimit = 0.0f;
    [SerializeField] protected float rightLimit = 0.0f;
    [SerializeField] protected float leftLimit = 0.0f;

    [SerializeField] protected float maxAlpha = 1.0f;
    [SerializeField] protected UIType uiType = UIType.None;

    protected FlickDirection up = null;
    protected FlickDirection down = null;
    protected FlickDirection right = null;
    protected FlickDirection left = null;

    private FlickDirection currentDir = null;
    private FlickDirection defaultDir = null;
    private FlickDirection[] flickDirections;

    protected Image image;
    protected UITween ui;
    protected Tween fadeOutActive = null;

    public IObservable<Unit> UpSubject => up.FlickSubject;
    public IObservable<Unit> DownSubject => down.FlickSubject;
    public IObservable<Unit> RightSubject => right.FlickSubject;
    public IObservable<Unit> LeftSubject => left.FlickSubject;

    public IObservable<Unit> FlickSubject { get; protected set; }

    public IReadOnlyReactiveProperty<float> DragUp => up.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragDown => down.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragRight => right.DragRatioRP;
    public IReadOnlyReactiveProperty<float> DragLeft => left.DragRatioRP;

    public IObservable<FlickDirection> Drag { get; protected set; }

    private ISubject<Unit> isPressed = new Subject<Unit>();
    public IObservable<Unit> IsPressed => isPressed;

    private ISubject<Unit> reactivated = new Subject<Unit>();
    public IObservable<Unit> Reactivated => reactivated;

    /// <summary>
    /// Notifies the end of dragging UI.
    /// </summary>
    /// <typeparam name="bool">result of drag if the flick is applied(true) or canceled(false)</typeparam>
    private ISubject<bool> isReleased = new Subject<bool>();
    public IObservable<bool> IsReleased => isReleased;

    private Vector2 pressPos = Vector2.zero;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;

    protected float uiAlpha = 1f;

    protected override void Awake()
    {
        uiAlpha = DataStoreAgent.Instance.GetSettingData(uiType);

        image = GetComponent<Image>();
        fade = new FadeTween(image, uiAlpha * maxAlpha, true);
        ui = new UITween(gameObject);

        SetFlicks();
        flickDirections = new FlickDirection[] { up, down, right, left };

        // Set image.sprite as default icon of the handling UI
        defaultDir = flickDirections.Where(x => x?.sprite == image.sprite).FirstOrDefault();

        FlickSubject = Merge(flick => flick.FlickSubject);
        Drag = Merge(flick => flick.Drag);
    }

    /// <summary>
    /// Fade-out flicking button but reactivate again on complete if the UI is still active.
    /// </summary>
    public void FadeOutActive(float duration = 0.2f, bool isValidOnPause = false)
    {
        if (!isActive) return;
        isActive = false;

        fadeOutActive = fade.ToAlpha(0, duration)
            .OnComplete(() =>
            {
                InitImage();
                Clear();
                FadeIn(0.1f).Play();
                reactivated.OnNext(Unit.Default);
            })
            .SetUpdate(isValidOnPause)
            .Play();
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

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isPressed.OnNext(Unit.Default);
        pressPos = eventData.position;
        UpdateImage(Vector2.zero);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Release(DragVector(eventData.position));
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        UpdateImage(DragVector(eventData.position));
    }

    public void Release(Vector2 dragVector, float duration = 0.2f)
    {
        bool isApplied = currentDir?.Release(dragVector, duration) ?? false;
        isReleased.OnNext(isApplied);
    }

    public void Cancel(float duration = 0.2f)
    {
        if (!isActive) return;

        ui.MoveBack(duration).Play();
        ui.Resize(0.5f, duration).Play();
        FadeOutActive(duration);
    }

    public override void Activate()
    {
        defaultDir.SetSprite();
        ui.ResetSize();
        ui.ResetPos();
        base.Activate();
        image.raycastTarget = true;
    }

    public override void Inactivate()
    {
        image.raycastTarget = false;
        Clear();
        base.Inactivate();
    }

    protected override void OnFadeIn()
    {
        InitImage();
        image.raycastTarget = true;
    }

    protected override void OnFadeOut()
    {
        fadeOutActive?.Kill();
        image.raycastTarget = false;
    }

    public override Tween FadeOut(float duration = 0.2f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
        => base.FadeOut(duration, onPlay,
            () =>
            {
                (onComplete ?? (() => { }))();
                Clear();
            },
            isContinuous);

    public void UpdateImage(Vector2 dragVector)
    {
        currentDir = GetDirection(dragVector);
        currentDir?.UpdateImage(dragVector);
    }

    private FlickDirection GetDirection(Vector2 dragVector)
    {
        FlickDirection dir = null;

        if (Mathf.Abs(dragVector.x) >= Mathf.Abs(dragVector.y))
        {
            if (dragVector.x > 0) dir = right;
            if (dragVector.x < 0) dir = left;

            if (dir == null)
            {
                if (dragVector.y > 0) dir = up;
                if (dragVector.y < 0) dir = down;
            }
        }
        else
        {
            if (dragVector.y > 0) dir = up;
            if (dragVector.y < 0) dir = down;

            if (dir == null)
            {
                if (dragVector.x > 0) dir = right;
                if (dragVector.x < 0) dir = left;
            }
        }

        return dir ?? defaultDir ?? flickDirections.Where(x => x != null).First();
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
        protected Tween effectTween;
        public Sprite sprite { get; protected set; }
        public string text { get; protected set; }
        protected float limit;

        protected ReactiveProperty<float> dragRatio = new ReactiveProperty<float>(0.0f);
        public IReadOnlyReactiveProperty<float> DragRatioRP => dragRatio;
        public float Ratio => dragRatio.Value;

        public IObservable<FlickDirection> Drag;

        protected ISubject<Unit> flickSubject = new Subject<Unit>();
        public IObservable<Unit> FlickSubject => flickSubject;

        protected FlickDirection(FlickInteraction flick, Sprite sprite, string text, float limit)
        {
            this.flick = flick;
            this.fade = flick.fade;
            this.ui = flick.ui;
            this.sprite = sprite;
            this.text = text;
            this.limit = limit;

            Drag = DragRatioRP.Select(_ => this);
        }

        protected virtual Vector2 Destination => Vector2.zero;

        /// <summary>
        /// Returns drag directional factor limited by 'limit' property.
        /// </summary>
        protected abstract Vector2 LimitedVec(Vector2 dragVector);

        private void FlickOnNext()
        {
            flickSubject.OnNext(Unit.Default);
        }

        public void SetSprite()
        {
            effectTween?.Kill();
            fade.sprite = sprite;
        }

        /// <summary>
        /// Change sprite, move by drag directional factor and set alpha according to drag directional ratio.
        /// </summary>
        /// <param name="dragVector"></param>
        public void UpdateImage(Vector2 dragVector)
        {
            SetSprite();
            UpdateIconPos(dragVector);

            float degree = 1f - dragRatio.Value * 0.5f;
            ui.ResetSize(0.5f + degree);
            fade.SetAlpha(degree);
        }

        private void UpdateIconPos(Vector2 dragVector)
        {
            Vector2 limitedVec = LimitedVec(dragVector);
            ui.SetPosOffset(limitedVec);
            if (flick.isActive) dragRatio.SetValueAndForceNotify(limitedVec.magnitude / limit);
        }

        /// <summary>
        /// Applies or cancels flick according to drag distance on release dragging.
        /// </summary>
        /// <returns>true: flick applied, false: flick canceled</returns>
        public bool Release(Vector2 dragVector, float duration = 0.2f)
        {
            UpdateIconPos(dragVector);

            if (dragRatio.Value < 0.5f)
            {
                flick.Cancel(duration);
                return false;
            }

            effectTween = DOTween.Sequence()
                .Append(ui.MoveOffset(Destination, duration))
                .Join(fade.In(duration))
                .Append(ui.Resize(1.5f, duration))
                .InsertCallback(duration, () => flick.FadeOutActive(duration, true))
                .SetUpdate(false)
                .Play();

            FlickOnNext();
            return true;
        }
    }

    protected class FlickUp : FlickDirection
    {
        protected FlickUp(FlickInteraction flick) : base(flick, flick.upSprite, flick.upText, flick.upLimit) { }

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
        protected FlickDown(FlickInteraction flick) : base(flick, flick.downSprite, flick.downText, flick.downLimit) { }

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
        protected FlickRight(FlickInteraction flick) : base(flick, flick.rightSprite, flick.rightText, flick.rightLimit) { }

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
        protected FlickLeft(FlickInteraction flick) : base(flick, flick.leftSprite, flick.leftText, flick.leftLimit) { }

        public static FlickLeft New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickLeft(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.leftLimit > 0.0f && flick.leftSprite != null;

        protected override Vector2 Destination => new Vector2(-limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Clamp(dragVector.x, -limit, 0), 0);
    }
}