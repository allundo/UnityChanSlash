using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JabButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [SerializeField] protected Button button = default;

    [SerializeField] protected float drawDistance = 10.0f;
    [SerializeField] protected float pushDistance = 10.0f;
    [SerializeField] protected float minReleaseSpeed = 15.0f;
    [SerializeField] protected float maxReleaseSpeed = 30.0f;
    [SerializeField] protected float endDistance = 141.4f;

    protected RectTransform rectTransform = default;

    protected float convertDragDistance;

    protected Vector2 dir;
    protected Vector2 defaultPos;
    protected Vector2 defaultSize;

    protected float distance = 0f;
    protected float releaseSpeed = 0f;
    protected Vector2 dragStartPos;

    protected bool goToDestination = false;
    protected bool isDragEnabled = true;
    protected bool isClickEnabled = true;

    protected float GetVec2Component(Vector2 src) => Vector2.Dot(src, dir);

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = button.GetComponent<RectTransform>();
        convertDragDistance = transform.parent.gameObject.GetComponent<RectTransform>().rect.height / Screen.height * 2.0f;
        defaultPos = dragStartPos = rectTransform.anchoredPosition;
        dir = -defaultPos.normalized;
        defaultSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        if (goToDestination && releaseSpeed == 0.0f)
        {
            distance *= 0.5f;
            if (distance < 0.0001f)
            {
                goToDestination = false;
                rectTransform.sizeDelta = defaultSize;
                ExecuteEvents.Execute<Selectable>(gameObject, null, (target, data) => target.OnDeselect(data));
            }
        }

        distance += releaseSpeed;

        if (distance > endDistance)
        {
            releaseSpeed = distance = 0.0f;
            goToDestination = false;
            rectTransform.sizeDelta = defaultSize;
            // ExecuteEvents.Execute<Selectable>(gameObject, null, (target, data) => target.OnDeselect(data));
        }


        if (distance > 0.0f)
        {
            rectTransform.sizeDelta = defaultSize * (endDistance - distance) / endDistance;
        }

        rectTransform.anchoredPosition = defaultPos + dir * distance;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isClickEnabled)
        {
            PushShoot(minReleaseSpeed);
        }
        isClickEnabled = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
        isClickEnabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (goToDestination || !isDragEnabled) return;

        distance = CalcDistance(eventData.position - dragStartPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragEnabled = true;

        if (!goToDestination && distance == 0.0f) return;

        // Draw shoot
        if (distance < -drawDistance)
        {
            releaseSpeed = Mathf.Min(-distance, maxReleaseSpeed);
            goToDestination = true;
        }

        // Shoot cancel
        if (distance <= pushDistance)
        {
            goToDestination = true;
        }

        if (distance > pushDistance)
        {
            PushShoot(eventData.delta);
        }
    }

    protected void PushShoot(Vector2 dragDistance)
    {
        PushShoot(GetVec2Component(dragDistance * convertDragDistance));
    }

    protected void PushShoot(float distanceDelta)
    {
        releaseSpeed = Mathf.Max(distanceDelta, minReleaseSpeed);
        goToDestination = true;
    }

    protected float CalcDistance(Vector2 dragDistance)
    {
        float distance = GetVec2Component(dragDistance * convertDragDistance);

        float overDraw1 = distance + drawDistance;
        if (overDraw1 > 0.0f)
        {
            return distance;
        }

        float overDraw2 = distance + drawDistance * 2.0f;
        if (overDraw2 > 0.0f)
        {
            return overDraw1 * 0.5f - drawDistance;
        }

        float overDraw3 = distance + drawDistance * 3.0f;
        if (overDraw3 > 0.0f)
        {
            return overDraw2 * 0.25f - drawDistance * 1.5f;
        }

        return -drawDistance * 1.75f;
    }
}
