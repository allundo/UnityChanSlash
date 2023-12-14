using UnityEngine;
using DG.Tweening;

public class InfoRecordsUI : RecordsUI
{
    [SerializeField] protected GotTitle gotTitle = default;
    [SerializeField] protected InfoRecord info = default;
    [SerializeField] protected DiaryUI diaryUI = default;

    protected override void SetRecordsActive(bool isActive)
    {
        gotTitle.gameObject.SetActive(isActive);
        info.gameObject.SetActive(isActive);
        diaryUI.SetActive(false);
    }

    public void LoadRecords(DataStoreAgent.InfoRecord infoRecord)
    {

        gotTitle.SetValues(infoRecord.TitleList());
        gotTitle.ResetPosition(new Vector2(0f, 420f));

        info.SetValues(infoRecord.GetValues());
        info.ResetPosition(new Vector2(0f, -210f));

        diaryUI.SetDiaries(ResourceLoader.Instance.GetDiaries(infoRecord.readMessageIDs));

        slideInTween = DOTween.Sequence()
            .Join(gotTitle.SlideInTween())
            .Join(info.SlideInTween().SetDelay(0.1f))
            .AppendCallback(() =>
            {
                diaryUI.SetActive(true);
                NotifyEnd();
            })
            .AsReusable(gameObject);
    }
}
