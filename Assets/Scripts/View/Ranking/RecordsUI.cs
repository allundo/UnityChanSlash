using UnityEngine;
using DG.Tweening;
using TMPro;

public abstract class RecordsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpTitle = default;

    public string title
    {
        get { return tmpTitle.text; }
        set { tmpTitle.text = value; }
    }

    protected RectTransform rectTransform;
    protected BaseRecord[] records;

    protected abstract void SetRecordsActive(bool isActive);

    protected Tween slideInTween;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void DisplayRecords()
    {
        SetRecordsActive(true);
        slideInTween?.Restart();
    }

    public void HideRecords()
    {
        slideInTween?.Rewind();
        SetRecordsActive(false);
    }

    public Tween MoveX(float moveX, float duration)
    {
        return rectTransform.DOAnchorPosX(moveX, duration).SetRelative();
    }

    public void SetPosX(float posX)
    {
        var current = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(posX, current.y);
    }
}
