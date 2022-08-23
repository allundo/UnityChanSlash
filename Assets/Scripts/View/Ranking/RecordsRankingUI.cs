using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RecordsRankingUI : RecordsUI
{
    [SerializeField] private Image rank = default;

    protected override void Start()
    {
        rank.rectTransform.anchoredPosition = new Vector2(-20f + width, -80f);
        rank.gameObject.SetActive(false);

        records = new Image[10];
        records[0] = rank;

        var seq = DOTween.Sequence()
            .Join(rank.rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative());

        for (int i = 1; i < 10; i++)
        {
            records[i] = Instantiate(rank, transform);
            records[i].gameObject.name = "Rank" + (i + 1);
            records[i].rectTransform.anchoredPosition = new Vector2(-20f + i * 4 + width, -80f - 160f * i);
            seq.Join(records[i].rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative().SetDelay(0.01f * i));
        }

        slideInTween = seq.AsReusable(gameObject);
    }
}
