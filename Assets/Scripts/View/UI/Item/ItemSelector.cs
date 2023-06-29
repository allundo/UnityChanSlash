using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using System;
using DG.Tweening;

public class ItemSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private Sprite select = default;
    [SerializeField] private Sprite target = default;

    private Image image;
    private UITween ui;
    private RaycastHandler raycastHandler;

    private ISubject<Vector2> onDrag = new Subject<Vector2>();
    public IObservable<Vector2> OnDragMode => onDrag;

    private IReactiveProperty<bool> isLongPressing = new ReactiveProperty<bool>(false);
    public IObservable<Unit> OnLongPress => isLongPressing
        .Where(x => x)
        .SelectMany(_ => Observable.TimerFrame(90))
        .TakeUntil(isLongPressing.Where(x => !x))
        .RepeatUntilDestroy(this)
        .Select(_ =>
        {
            isLongPressing.Value = isDragOn = false;
            return Unit.Default;
        });

    private ISubject<Unit> onReleased = new Subject<Unit>();
    public IObservable<Unit> OnReleased => onReleased;

    private Vector2 startPos = Vector2.zero;
    private Vector2 dragVec(Vector2 screenPos) => screenPos - startPos;
    private bool IsOnCircle(Vector2 screenPos)
        => (ui.CurrentScreenPos - screenPos).magnitude <= ui.CurrentSize.x * 0.5f;

    private bool isDragOn = false;
    public bool isEquip { get; private set; } = false;

    void Awake()
    {
        image = GetComponent<Image>();

        ui = new UITween(gameObject);
        raycastHandler = new RaycastHandler(image);

        var OnDragStart = onDrag.Where(pos => dragVec(pos).magnitude > 50f).Select(_ => 0L);

        SetEnable(false);
        SetRaycast(false);
    }

    public void Disable()
    {
        isLongPressing.Value = isDragOn = false;
        SetEnable(false);
    }

    protected void SetEnable(bool isEnable)
    {
        image.enabled = enabled = isEnable;
    }

    public void Hide() => SetVisible(false);
    public void Show() => SetVisible(true);

    public void SetVisible(bool isVisible)
    {
        image.enabled = isVisible;
    }

    public void SetRaycast(bool isEnable)
    {
        image.raycastTarget = isEnable;
    }

    public ItemSelector SetPosition(Vector2 pos)
    {
        ui.SetPos(pos);
        return this;
    }

    public ItemSelector SetTarget(Vector2 pos)
    {
        image.sprite = target;
        ui.Resize(1f, 0.2f).Play();

        return SetPosition(pos);
    }
    public ItemSelector SetSelect(Vector2 pos, bool isEquip)
    {
        SetEnable(true);
        this.isEquip = isEquip;
        image.sprite = select;
        ui.Resize(1.4f, 0.2f).Play();
        AndroidUtil.Vibrate();

        return SetPosition(pos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isLongPressing.Value = false;

        if (!isDragOn)
        {
            raycastHandler.RaycastDrag(eventData);
            return;
        }

        onDrag.OnNext(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position;
        isDragOn = IsOnCircle(startPos);

        if (!isDragOn)
        {
            raycastHandler.RaycastPointerDown(eventData);
            return;
        }

        isLongPressing.Value = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isLongPressing.Value = false;

        if (!isDragOn)
        {
            raycastHandler.RaycastPointerUp(eventData);
            return;
        }

        onReleased.OnNext(Unit.Default);
        isDragOn = false;
    }
}
