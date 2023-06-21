using UnityEngine;
using TMPro;

public class ClearRecord : RankRecord
{
    [SerializeField] private TextMeshProUGUI title = default;
    [SerializeField] private TextMeshProUGUI wagesAmount = default;
    [SerializeField] private TextMeshProUGUI clearTime = default;
    [SerializeField] private TextMeshProUGUI defeat = default;

    protected override void SetFormats()
    {
        base.SetFormats();
        textObjects.Add(new TextObject(title));
        textFormats.Add(title => title.ToString());

        textObjects.Add(new TextObject(wagesAmount));
        textFormats.Add(wages => $"お給金: ￥{wages:#,0}");

        textObjects.Add(new TextObject(clearTime));
        textFormats.Add(sec => "踏破時間: " + Util.TimeFormat((int)sec));

        textObjects.Add(new TextObject(defeat));
        textFormats.Add(count => "敵撃破数: " + count);
    }

    private string ValueFormat(int sec)
    {
        int min = sec / 60;
        int hour = min / 60;
        return $"{hour,3:D}:{min % 60:00}:{sec % 60:00}";
    }

    public void SetValues(int rank, DataStoreAgent.ClearRecord clearRecord)
        => base.SetValues(rank, clearRecord.title, clearRecord.wagesAmount, clearRecord.clearTimeSec, clearRecord.defeatCount);
}
