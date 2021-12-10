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

    private ISubject<Unit> onReleased = new Subject<Unit>();
    public IObservable<Unit> OnReleased => onReleased;

    private Vector2 startPos = Vector2.zero;
    private Vector2 dragVec(Vector2 screenPos) => screenPos - startPos;
    private bool IsOnCircle(Vector2 screenPos)
        => (ui.CurrentScreenPos - screenPos).magnitude <= ui.CurrentSize.x * 0.5f;

    private bool isDragOn = false;

    void Awake()
    {
        image = GetComponent<Image>();

        ui = new UITween(gameObject);
        raycastHandler = new RaycastHandler(image);

        var OnDragStart = onDrag.Where(pos => dragVec(pos).magnitude > 50f).Select(_ => 0L);

        SetEnable(false);
        SetRaycast(false);
    }

    public void Disable() => SetEnable(false);
    public void Enable() => SetEnable(true);

    public void SetEnable(bool isEnable)
    {
        image.enabled = isEnable;
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
    public ItemSelector SetSelect(Vector2 pos)
    {
        image.sprite = select;
        ui.Resize(1.4f, 0.2f).Play();

        return SetPosition(pos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragOn)
        {
            raycastHandler.RaycastDrag(eventData);
            return;
        }

        onDrag.OnNext(eventData.position);
    }

#if UNITY_EDITOR
    // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
    private InputControl ic = new InputControl();
#endif

    public void OnPointerDown(PointerEventData eventData)
    {

#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif
        startPos = eventData.position;
        isDragOn = IsOnCircle(startPos);

        if (!isDragOn) raycastHandler.RaycastPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif

        if (!isDragOn)
        {
            raycastHandler.RaycastPointerUp(eventData);
            return;
        }

        onReleased.OnNext(Unit.Default);
        isDragOn = false;
    }
}
