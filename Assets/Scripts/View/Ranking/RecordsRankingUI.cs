using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class RecordsRankingUI : RecordsUI
{
    private Vector2 rankBasePos;

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rankBasePos = new Vector2(-20f + width, -80f);
    }

    public void LoadRecords<T>(List<T> rankRecords) where T : DataStoreAgent.DataArray
    {
        var length = rankRecords.Count;

        if (length > 0)
        {
            records = new BaseRecord[length];
            record.SetValues(rankRecords[0].GetValues(1));
        }
        else
        {
            records = new BaseRecord[1];
        }

        records[0] = record;
        records[0].rectTransform.anchoredPosition = rankBasePos;

        var seq = DOTween.Sequence()
            .Join(records[0].rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative());

        for (int i = 1; i < length; i++)
        {
            int rank = i + 1;
            records[i] = Instantiate(record, transform);
            records[i].gameObject.name = "Rank" + rank;
            records[i].SetValues(rankRecords[i].GetValues(rank));
            records[i].rectTransform.anchoredPosition = rankBasePos + new Vector2(4 * i, -160f * i);
            seq.Join(records[i].rectTransform.DOAnchorPosX(-width, 0.5f).SetEase(Ease.OutQuart).SetRelative().SetDelay(0.01f * i));
        }

        SetRecordsActive(false);

        slideInTween = seq.AsReusable(gameObject);
    }
}
