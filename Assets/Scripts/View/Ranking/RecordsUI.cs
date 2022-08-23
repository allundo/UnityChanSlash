using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class RecordsUI : MonoBehaviour
{
    protected RectTransform rectTransform;
    protected Image[] records;

    protected virtual void SetRecordsActive(bool isActive)
        => records.ForEach(record => record.gameObject.SetActive(isActive));

    protected float width;
    protected Tween slideInTween;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        width = Screen.width;
    }

    protected virtual void Start()
    {
        records = new Image[] { transform.GetChild(1).GetComponent<Image>() };

        records[0].rectTransform.anchoredPosition = new Vector2(width, 0f);
        slideInTween = records[0].rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative().AsReusable(gameObject);
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
