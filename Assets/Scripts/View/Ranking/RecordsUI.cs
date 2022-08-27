using UnityEngine;
using DG.Tweening;

public class RecordsUI : MonoBehaviour
{
    [SerializeField] protected BaseRecord record = default;

    protected RectTransform rectTransform;
    protected BaseRecord[] records;

    protected virtual void SetRecordsActive(bool isActive)
        => records.ForEach(record => record.gameObject.SetActive(isActive));

    protected float width = 1080f;
    protected Tween slideInTween;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void LoadRecords<T>(T rankRecord) where T : DataStoreAgent.DataArray
    {
        records = new BaseRecord[] { record };

        records[0].rectTransform.anchoredPosition = new Vector2(width, 0f);
        slideInTween = records[0].rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative().AsReusable(gameObject);

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
