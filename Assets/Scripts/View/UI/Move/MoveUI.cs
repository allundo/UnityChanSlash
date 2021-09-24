using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MoveUI : MonoBehaviour
{
    [SerializeField] protected MoveButton moveButton = default;

    protected RectTransform rectTransform;
    protected Vector2 defaultSize;

    protected bool isActive = false;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultSize = rectTransform.sizeDelta;
    }

    protected virtual void Start()
    {
        moveButton.Inactivate();
        gameObject.SetActive(false);
    }

    public void Resize(float ratioX, float ratioY)
    {
        rectTransform.sizeDelta = new Vector2(ratioX * defaultSize.x, ratioY * defaultSize.y);
    }

    public void Activate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        moveButton.FadeIn(0.2f).Play();
    }

    public void Inactivate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (!isActive) return;

        isActive = false;
        moveButton.FadeOut(0.2f, null, () => gameObject.SetActive(false)).Play();
    }

    public void SetActive(bool value, bool isFighting = false)
    {
        if (value)
        {
            Activate(isFighting);
        }
        else
        {
            Inactivate(isFighting);
        }
    }

    protected void Execute<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunc) where T : IEventSystemHandler
    {
        ExecuteEvents.Execute<T>(moveButton.gameObject, eventData, eventFunc);
    }
}