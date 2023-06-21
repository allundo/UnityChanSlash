using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class RecordsRankingUI : RecordsUI
{
    [SerializeField] protected BaseRecord record = default;

    public bool hasRecord { get; protected set; } = false;

    protected override void SetRecordsActive(bool isActive)
        => records.ForEach(record => record.gameObject.SetActive(isActive));

    public void LoadRecords<T>(List<T> rankRecords) where T : DataStoreAgent.DataArray
    {
        var length = rankRecords.Count;
        hasRecord = length > 0;

        if (hasRecord)
        {
            records = new BaseRecord[length];
            record.SetValues(rankRecords[0].GetValues(1));
        }
        else
        {
            records = new BaseRecord[1];
        }

        var rankBaseOffset = new Vector2(-4f * length / 2, -80f);

        records[0] = record;
        records[0].ResetPosition(rankBaseOffset);

        var seq = DOTween.Sequence()
            .Join(records[0].SlideInTween());

        for (int i = 1; i < length; i++)
        {
            int rank = i + 1;
            records[i] = Instantiate(record, transform);
            records[i].gameObject.name = "Rank" + rank;
            records[i].SetValues(rankRecords[i].GetValues(rank));
            records[i].ResetPosition(rankBaseOffset + new Vector2(4 * i, -160f * i));
            seq.Join(records[i].SlideInTween().SetDelay(0.01f * i));
        }

        SetRecordsActive(false);

        slideInTween = seq.AsReusable(gameObject);
    }
}
