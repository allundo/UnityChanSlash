using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HandleButton : MonoBehaviour
{
    [SerializeField] Sprite handle = default;
    [SerializeField] Sprite circle = default;
    [SerializeField] float maxAlpha = 1.0f;
    [SerializeField] RectTransform textRT = default;

    protected RectTransform rectTransform;
    protected Image image;
    private Vector2 defaultSize;

    private Tween cycle;
    private Tween expand;
    private bool isPressed = false;

    public Vector2 Position => rectTransform.anchoredPosition;
    public Vector2 Size => rectTransform.sizeDelta;
    public float Radius => Size.x * 0.5f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Start()
    {
        defaultSize = Size;
        cycle = GetRotate(-90.0f, 5.0f, true).SetEase(Ease.Linear);
        expand = GetResize(1.5f, 0.2f, true);

        textRT.gameObject.SetActive(false);
    }

    private void ResetSize()
    {
        rectTransform.sizeDelta = defaultSize;
    }

    private Tween GetRotate(float angle, float duration, bool isLoop = false)
    {
        Tween rotate = rectTransform.DOLocalRotate(new Vector3(0, 0, angle), duration);

        return isLoop ? rotate.SetLoops(-1).AsReusable(gameObject) : rotate;
    }

    private Tween GetResize(float ratio = 1.5f, float duration = 0.2f, bool isReusable = false)
    {
        Tween resize = rectTransform.DOSizeDelta(defaultSize * ratio, duration);

        return isReusable ? resize.AsReusable(gameObject) : resize;
    }

    public void PressButton()
    {
        if (isPressed) return;

        isPressed = true;
        image.sprite = circle;
        cycle.Restart();
        expand.Restart();
    }

    public void ReleaseButton()
    {
        if (!isPressed) return;

        isPressed = false;
        cycle.Rewind();
        expand.Rewind();
        ResetSize();
        SetAlpha(1.0f);
        image.sprite = handle;

        textRT.gameObject.SetActive(false);
    }

    public void UpdateImage(float dragRatio)
    {
        textRT.gameObject.SetActive(dragRatio > 0.5f);
        SetAlpha(1.0f - dragRatio);
    }

    public void SetAlpha(float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }

    public void Activate(float alpha)
    {
        SetAlpha(alpha);
        gameObject.SetActive(true);
    }

    public void Inactivate()
    {
        gameObject.SetActive(false);
        textRT.gameObject.SetActive(false);
    }
}
