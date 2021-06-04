using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

public class FightCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    [SerializeField] private JabButton childButton = default;
    [SerializeField] private readonly float maxAlpha = 0.8f;

    private RectTransform rectTransform;
    private RawImage rawImage;

    private float alpha = 0.0f;
    private bool isActive = true;

    private Vector2 UICenter;
    private Vector2 screenCenter;
    private float radius;

    public ISubject<Unit> AttackSubject { get; protected set; } = new Subject<Unit>();

    private Vector2 UIPos(Vector2 screenPos) => screenPos - screenCenter;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).magnitude < radius;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();

        radius = rectTransform.rect.height / 2 - 10;

        UICenter = rectTransform.anchoredPosition;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2) + UICenter;

        SetAlpha(0.0f);
        gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += isActive ? 0.1f : -0.1f;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
            return;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;
            gameObject.SetActive(false);
            return;
        }

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = rawImage.color;
        rawImage.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;

        AttackSubject.OnNext(Unit.Default);
        childButton.Inactivate();
        if (InCircle(eventData.position)) Debug.Log("position UP: " + eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActive) return;

        childButton.Activate();
        childButton.SetPos(UIPos(eventData.position));
        if (InCircle(eventData.position)) Debug.Log("position DOWN: " + eventData.position);
    }

    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Inactivate()
    {
        isActive = false;
    }

    public void SetActive(bool value)
    {
        if (value)
        {
            Activate();
        }
        else
        {
            Inactivate();
        }
    }
}
