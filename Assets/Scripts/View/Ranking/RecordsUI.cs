using UnityEngine;
using DG.Tweening;
using TMPro;

public class RecordsUI : MonoBehaviour
{
    [SerializeField] protected BaseRecord record = default;
    [SerializeField] private TextMeshProUGUI tmpTitle = default;

    public string title
    {
        get { return tmpTitle.text; }
        set { tmpTitle.text = value; }
    }

    protected RectTransform rectTransform;
    protected BaseRecord[] records;

    protected virtual void SetRecordsActive(bool isActive)
        => records.ForEach(record => record.gameObject.SetActive(isActive));

    protected float width = 1080f;
    protected Tween slideInTween;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        records = new BaseRecord[] { record };
    }

    public void LoadRecords<T>(T rankRecord) where T : DataStoreAgent.DataArray
    {
        records[0].ResetPosition();
        slideInTween = records[0].SlideInTween().AsReusable(gameObject);

        records[0].gameObject.SetActive(false);
    }

    public void DisplayRecords()
    {
        SetRecordsActive(true);
        slideInTween.Restart();
    }

    public void HideRecords()
    {
        slideInTween.Rewind();
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

    public void CompleteTween() => slideInTween.Complete(true);
}
