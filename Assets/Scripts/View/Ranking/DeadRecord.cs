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
        textObjects.Add(floor);
        textFormats.Add(floor => "地下" + floor + "階");

        textObjects.Add(moneyAmount);
        textFormats.Add(money => $"・アイテム資産: ￥{money:#,0}");

        textObjects.Add(causeOfDeath);
        textFormats.Add(cause => "・死因:" + cause);
    }
}