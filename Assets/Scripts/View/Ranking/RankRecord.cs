using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class RankRecord : BaseRecord
{
    [SerializeField] private TextMeshProUGUI noData = default;
    [SerializeField] private Image rankImage = default;
    private TextMeshProUGUI rank = default;


    protected override void Awake()
    {
        rank = rankImage.GetComponentInChildren<TextMeshProUGUI>();
        base.Awake();
    }

    protected override void SetFormats()
    {
        textObjects.Add(rank);
        textFormats.Add(rank => "第" + rank + "位");
    }

    protected override void SetActive(bool isActive)
    {
        noData.enabled = !isActive;
        rankImage.enabled = isActive;

        foreach (var obj in textObjects) obj.gameObject.SetActive(isActive);
    }
}