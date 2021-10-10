using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using System;

public class ItemSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private Sprite select = default;
    [SerializeField] private Sprite target = default;

    private RectTransform rectTransform;
    private Image image;

    private ISubject<Vector2> onDrag = new Subject<Vector2>();
    public IObservable<Vector2> OnDragMode => onDrag;

    private ISubject<Unit> onReleased = new Subject<Unit>();
    public IObservable<Unit> OnReleased => onReleased;

    private Vector2 startPos = Vector2.zero;
    private Vector2 dragVec(Vector2 screenPos) => screenPos - startPos;

    void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

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
        rectTransform.anchoredPosition = pos;
        return this;
    }

    public ItemSelector SetTarget(Vector2 pos)
    {
        image.sprite = target;

        return SetPosition(pos);
    }
    public ItemSelector SetSelect(Vector2 pos)
    {
        image.sprite = select;

        return SetPosition(pos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDrag.OnNext(eventData.position);
    }

#if UNITY_EDITOR
    // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
    private bool isFired = false;
    private bool CanFire()
    {
        if (isFired) return false;
        isFired = true;
        Observable.NextFrame().Subscribe(_ => isFired = false);
        return true;
    }
#endif

    public void OnPointerDown(PointerEventData eventData)
    {

#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!CanFire()) return;
#endif
        startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {

#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!CanFire()) return;
#endif
        onReleased.OnNext(Unit.Default);
    }
}
