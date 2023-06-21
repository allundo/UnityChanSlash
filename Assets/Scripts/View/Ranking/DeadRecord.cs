using UnityEngine;
using TMPro;

public class DeadRecord : RankRecord
{
    [SerializeField] private TextMeshProUGUI floor = default;
    [SerializeField] private TextMeshProUGUI moneyAmount = default;
    [SerializeField] private TextMeshProUGUI causeOfDeath = default;

    protected override void SetFormats()
    {
        base.SetFormats();
        textObjects.Add(new TextObject(floor));
        textFormats.Add(floor => "地下" + floor + "階");

        textObjects.Add(new TextObject(moneyAmount));
        textFormats.Add(money => $"アイテム資産: ￥{money:#,0}");

        textObjects.Add(new TextObject(causeOfDeath));
        textFormats.Add(cause => cause.ToString());
    }

    public void SetValues(int rank, DataStoreAgent.DeadRecord deadRecord)
        => base.SetValues(rank, deadRecord.floor, deadRecord.moneyAmount, deadRecord.causeOfDeath);
}