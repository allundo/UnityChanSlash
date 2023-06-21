using UnityEngine;
using TMPro;

public class InfoRecord : BaseRecord
{
    [SerializeField] private TextMeshProUGUI minStep = default;
    [SerializeField] private TextMeshProUGUI maxMap = default;
    [SerializeField] private TextMeshProUGUI playTime = default;
    [SerializeField] private TextMeshProUGUI clearCount = default;
    [SerializeField] private TextMeshProUGUI deadCount = default;

    protected override void SetFormats()
    {
        textObjects.Add(new TextObject(minStep));
        textFormats.Add(steps => $"{steps:#,0}");

        textObjects.Add(new TextObject(maxMap));
        textFormats.Add(mapComp => Util.PercentFormat((float)mapComp));

        textObjects.Add(new TextObject(playTime));
        textFormats.Add(sec => Util.TimeFormat((int)sec));

        textObjects.Add(new TextObject(clearCount));
        textFormats.Add(count => count.ToString());

        textObjects.Add(new TextObject(deadCount));
        textFormats.Add(count => count.ToString());
    }
}