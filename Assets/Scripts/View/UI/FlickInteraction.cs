using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class FlickInteraction : MonoBehaviour
{
    [SerializeField] private HandleButton parent = default;
    [SerializeField] private Sprite upSprite = null;
    [SerializeField] private Sprite downSprite = null;
    [SerializeField] private Sprite leftSprite = null;
    [SerializeField] private Sprite rightSprite = null;

    [SerializeField] private float upLimit = 0.0f;
    [SerializeField] private float downLimit = 0.0f;
    [SerializeField] private float rightLimit = 0.0f;
    [SerializeField] private float leftLimit = 0.0f;

    [SerializeField] float maxAlpha = 1.0f;

    private FlickDirection up = null;
    private FlickDirection down = null;
    private FlickDirection right = null;
    private FlickDirection left = null;

    private FlickDirection currentDir = null;

    private RectTransform rectTransform;
    private Image image;

    private Vector2 defaultSize;
    private Vector2 defaultPos;
    private Color defaultColor;

    private Vector2 dragVector;

    public ISubject<Unit> UpSubject => up.FlickSubject;
    public ISubject<Unit> DownSubject => down.FlickSubject;
    public ISubject<Unit> RightSubject => right.FlickSubject;
    public ISubject<Unit> LeftSubject => left.FlickSubject;

    protected IReactiveProperty<bool> isHandOn = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> IsHandOn => isHandOn;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        up = FlickUp.New(this);
        down = FlickDown.New(this);
        right = FlickRight.New(this);
        left = FlickLeft.New(this);
    }

    void Start()
    {
        defaultSize = rectTransform.sizeDelta;
        defaultPos = rectTransform.anchoredPosition;
        defaultColor = image.color;

        gameObject.SetActive(false);
    }

    private void ResetSize()
    {
        rectTransform.sizeDelta = defaultSize;
    }

    private void SetAlpha(float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }

    private void UpdateParentImage(float dragRatio)
    {
        isHandOn.Value = dragRatio > 0.5f;

        parent.SetAlpha(1.0f - dragRatio);
        parent.textRT.gameObject.SetActive(isHandOn.Value);
    }

    private Tween GetFadeOutToInactive(float duration = 0.2f, TweenCallback OnPlay = null)
    {
        OnPlay = OnPlay ?? Clear;

        return
            GetToAlpha(0.0f, duration)
                .OnPlay(OnPlay)
                .OnComplete(() => gameObject.SetActive(false));
    }

    private Tween GetResize(float ratio = 1.5f, float duration = 0.2f)
    {
        return
            rectTransform
                .DOSizeDelta(defaultSize * ratio, duration)
                .OnComplete(() => rectTransform.sizeDelta = defaultSize);
    }

    private Tween GetMove(Vector2 destination, float duration = 0.2f)
    {
        return rectTransform.DOAnchorPos(destination + defaultPos, duration);
    }

    private Tween GetToAlpha(float alpha, float duration = 0.2f)
    {
        return DOTween.ToAlpha(() => image.color, c => image.color = c, alpha * maxAlpha, duration);
    }

    public void Release(Vector2 dragVector, float duration = 0.2f)
    {
        currentDir?.Release(dragVector, duration);
    }

    public void Cancel(float duration = 0.2f)
    {
        GetMove(Vector2.zero, duration).Play();
        GetResize(0.5f, duration).Play();
        GetFadeOutToInactive(duration).Play();
    }

    /// <summary>
    /// Set image position by offset vector from initial position
    /// </summary>
    /// <param name="pos">Offset vector from initial position</param>
    private void SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos + defaultPos;
    }

    private void Activate(Vector2 dragVector)
    {
        if (currentDir != null) return;

        ResetSize();
        gameObject.SetActive(true);
    }

    public void Inactivate(float duration = 0.2f)
    {
        if (currentDir == null) return;

        GetFadeOutToInactive(duration).Play();
    }

    public void UpdateImage(Vector2 dragVector)
    {
        Activate(dragVector);

        currentDir = GetDirection(dragVector);
        currentDir?.UpdateImage(dragVector);
        parent.PressButton();
    }

    private FlickDirection GetDirection(Vector2 dragVector)
    {
        FlickDirection dir;
        if (Mathf.Abs(dragVector.x) >= Mathf.Abs(dragVector.y))
        {
            dir = dragVector.x > 0 ? right : left;
            dir = dir ?? (dragVector.y > 0 ? up : down);
        }
        else
        {
            dir = dragVector.y > 0 ? up : down;
            dir = dir ?? (dragVector.x > 0 ? right : left);
        }

        return dir;
    }

    private void Clear()
    {
        currentDir = null;
        isHandOn.Value = false;
        parent.ReleaseButton();
    }

    private abstract class FlickDirection
    {
        protected FlickInteraction flick;
        protected Sprite sprite;
        protected float limit;
        public ISubject<Unit> FlickSubject { get; protected set; } = new Subject<Unit>();

        protected FlickDirection(FlickInteraction flick, Sprite sprite, float limit)
        {
            this.flick = flick;
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

        /// <summary>
        /// Change sprite, move by drag directional factor and set alpha according to drag directional ratio.
        /// </summary>
        /// <param name="dragVector"></param>
        public void UpdateImage(Vector2 dragVector)
        {
            flick.image.sprite = sprite;

            Vector2 limitedVec = LimitedVec(dragVector);
            float dragRatio = DragRatio(limitedVec.magnitude);

            flick.SetPos(limitedVec);
            flick.SetAlpha(dragRatio);
            flick.UpdateParentImage(dragRatio);
        }

        /// <summary>
        /// Returns drag directional ratio to 'limit' property.
        /// </summary>
        /// <param name="abs">Absolute value of drag directional factor</param>
        private float DragRatio(float abs) => abs / limit;
        private float DragRatio(Vector2 dragVector) => DragRatio(LimitedVec(dragVector).magnitude);

        public void Release(Vector2 dragVector, float duration = 0.2f)
        {
            if (DragRatio(dragVector) < 0.5f)
            {
                flick.Cancel(duration);
                return;
            }

            DOTween.Sequence()
                .Append(flick.GetMove(Destination, duration))
                .Join(flick.GetToAlpha(1.0f, duration))
                .Append(flick.GetResize(1.5f, duration))
                .Join(flick.GetFadeOutToInactive(duration))
                .Play();

            FlickOnNext();
        }
    }

    private class FlickUp : FlickDirection
    {
        private FlickUp(FlickInteraction flick) : base(flick, flick.upSprite, flick.upLimit) { }

        public static FlickUp New(FlickInteraction flick)
        {
            if (flick.upLimit <= 0.0f || flick.upSprite == null) return null;

            return new FlickUp(flick);
        }

        protected override Vector2 Destination => new Vector2(0, limit * 1.5f);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(0, Mathf.Min(limit, dragVector.y));
    }

    private class FlickDown : FlickDirection
    {
        private FlickDown(FlickInteraction flick) : base(flick, flick.downSprite, flick.downLimit) { }

        public static FlickDown New(FlickInteraction flick)
        {
            if (flick.downLimit <= 0.0f || flick.downSprite == null) return null;

            return new FlickDown(flick);
        }

        protected override Vector2 Destination => new Vector2(0, -limit * 1.5f);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(0, Mathf.Max(-limit, dragVector.y));
    }

    private class FlickRight : FlickDirection
    {
        private FlickRight(FlickInteraction flick) : base(flick, flick.rightSprite, flick.rightLimit) { }

        public static FlickRight New(FlickInteraction flick)
        {
            if (flick.rightLimit <= 0.0f || flick.rightSprite == null) return null;

            return new FlickRight(flick);
        }

        protected override Vector2 Destination => new Vector2(limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Min(limit, dragVector.x), 0);
    }

    private class FlickLeft : FlickDirection
    {
        private FlickLeft(FlickInteraction flick) : base(flick, flick.leftSprite, flick.leftLimit) { }

        public static FlickLeft New(FlickInteraction flick)
        {
            if (flick.leftLimit <= 0.0f || flick.leftSprite == null) return null;

            return new FlickLeft(flick);
        }

        protected override Vector2 Destination => new Vector2(-limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Max(-limit, dragVector.x), 0);
    }
}