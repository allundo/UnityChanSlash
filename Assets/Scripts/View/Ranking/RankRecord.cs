using UnityEngine;
using TMPro;
using DG.Tweening;

public abstract class RankRecord : BaseRecord
{
    [SerializeField] private TextMeshProUGUI noData = default;
    [SerializeField] private RankText rankText = default;

    protected override void SetFormats()
    {
        textObjects.Add(rankText);
        textFormats.Add(rank => (int)rank > 0 ? "第" + rank + "位" : "ランク外");
    }

    protected override void SetActive(bool isActive)
    {
        noData.enabled = !isActive;
        rankText.SetActive(isActive);

        foreach (var obj in textObjects) obj.SetActive(isActive);
    }

    public Tween RankEffect(int rank) => rankText.RankEffect(rank);
    public Tween RankPunchEffect(int rank) => rankText.RankPunchEffect(rank);
    public void SetRankEnable(bool isEnable) => rankText.SetTextEnable(isEnable);
}
