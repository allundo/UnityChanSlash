using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoveUI : MonoBehaviour
{
    [SerializeField] protected MoveButton moveButton = default;
    [SerializeField] private float maxAlpha = 0.4f;

    protected RectTransform rectTransform;
    private Image image;
    protected Vector2 defaultSize;

    private float alpha = 0.0f;
    protected bool isActive = false;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        defaultSize = rectTransform.sizeDelta;
    }

    protected virtual void Start()
    {
        SetAlpha(0.0f);
        moveButton.Inactivate();
        gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += isActive ? 0.05f : -0.05f;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
            return;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;

            moveButton.Inactivate();
            gameObject.SetActive(false);

            return;
        }

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        moveButton.SetAlpha(alpha);
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

        moveButton.Activate(alpha);
    }

    public void Inactivate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (!isActive) return;

        isActive = false;
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