using UnityEngine;
using DG.Tweening;

public class InfoRecordsUI : RecordsUI
{
    [SerializeField] protected GotTitle gotTitle = default;
    [SerializeField] protected InfoRecord info = default;

    protected override void SetRecordsActive(bool isActive)
    {
        gotTitle.gameObject.SetActive(isActive);
        info.gameObject.SetActive(isActive);
    }

    public void LoadRecords(DataStoreAgent.InfoRecord infoRecord)
    {
        gotTitle.SetValues(infoRecord.TitleList());
        gotTitle.ResetPosition(new Vector2(0f, 420f));

        info.SetValues(infoRecord.GetValues());
        info.ResetPosition(new Vector2(0f, -210f));

        slideInTween = DOTween.Sequence()
            .Join(gotTitle.SlideInTween())
            .Join(info.SlideInTween().SetDelay(0.1f))
            .AsReusable(gameObject);
    }
}
