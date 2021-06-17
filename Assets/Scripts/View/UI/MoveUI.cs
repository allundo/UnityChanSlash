using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoveUI : MonoBehaviour
{
    [SerializeField] protected MoveButton moveButton = default;
    [SerializeField] private float maxAlpha = 0.4f;

    private RectTransform rectTransform;
    private Image image;

    private float alpha = 0.0f;
    private bool isActive = false;

    private Vector2 UICenter;
    private Vector2 screenCenter;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - screenCenter - UICenter;
    private Vector2 ScreenVec(Vector2 screenPos) => screenPos - screenCenter;


    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    protected virtual void Start()
    {
        UICenter = rectTransform.anchoredPosition;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

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