using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class FlickInteraction : MonoBehaviour
{
    [SerializeField] protected HandleButton parent = default;
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


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        SetFlicks();
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
        parent.ReleaseButton();
    }

    protected abstract class FlickDirection
    {
        protected FlickInteraction flick;
        protected Sprite sprite;
        protected float limit;
        protected RectTransform textRT;

        public ISubject<Unit> FlickSubject { get; protected set; } = new Subject<Unit>();

        protected FlickDirection(FlickInteraction flick, Sprite sprite, float limit, RectTransform textRT)
        {
            this.flick = flick;
            this.sprite = sprite;
            this.limit = limit;
            this.textRT = textRT;
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
            UpdateParentImage(dragRatio);
        }

        /// <summary>
        /// Hide parent circle image and show command text when flick type is determinded
        /// </summary>
        /// <param name="dragRatio"></param>
        protected virtual void UpdateParentImage(float dragRatio)
        {
            flick.parent.SetAlpha(1.0f - dragRatio);
            textRT.gameObject.SetActive(dragRatio > 0.5f);
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

    protected class FlickUp : FlickDirection
    {
        protected FlickUp(FlickInteraction flick) : base(flick, flick.upSprite, flick.upLimit, flick.parent.upTextRT) { }

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
        protected FlickDown(FlickInteraction flick) : base(flick, flick.downSprite, flick.downLimit, flick.parent.downTextRT) { }

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
        protected FlickRight(FlickInteraction flick) : base(flick, flick.rightSprite, flick.rightLimit, flick.parent.rightTextRT) { }

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
        protected FlickLeft(FlickInteraction flick) : base(flick, flick.leftSprite, flick.leftLimit, flick.parent.leftTextRT) { }

        public static FlickLeft New(FlickInteraction flick)
        {
            return IsValid(flick) ? new FlickLeft(flick) : null;
        }

        protected static bool IsValid(FlickInteraction flick) => flick.leftLimit > 0.0f && flick.leftSprite != null;

        protected override Vector2 Destination => new Vector2(-limit * 1.5f, 0);
        protected override Vector2 LimitedVec(Vector2 dragVector) => new Vector2(Mathf.Clamp(dragVector.x, -limit, 0), 0);
    }
}