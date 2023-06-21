using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GotTitle : BaseRecord
{
    [SerializeField] private BGText prefabTitleTxt = default;

    protected override void SetFormats()
    {
        var count = PlayerCounter.Titles.Count;
        int columns = 4;
        int raws = count / columns;
        int remainder = count % columns;

        Vector2 bgSize = rectTransform.sizeDelta;
        Vector2 titleSize = new Vector2(bgSize.x / (float)columns, bgSize.y / (raws + 2f));
        Vector2 startOffset = new Vector2(-bgSize.x + titleSize.x, bgSize.y - 3f * titleSize.y) * 0.5f;

        for (int i = 0; i < count - remainder; i++)
        {
            textObjects.Add(Instantiate(startOffset + new Vector2(i % columns * titleSize.x, i / columns * -titleSize.y)));
            textFormats.Add(title => title?.ToString());
        }

        // Aline center
        float bottomRawY = startOffset.y - raws * titleSize.y;
        for (int i = 0; i < remainder; i++)
        {
            textObjects.Add(Instantiate(new Vector2(titleSize.x * (2 * i + 1 - remainder) * 0.5f, bottomRawY)));
            textFormats.Add(title => title?.ToString());
        }
    }

    private BGText Instantiate(Vector2 anchoredPosition)
    {
        var title = Instantiate(prefabTitleTxt, transform);
        var rt = title.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPosition;
        return title;
    }

    public override void SetValues(params object[] values)
    {
        base.SetValues(values);

        textObjects.ForEach(obj =>
        {
            if (obj.text == null)
            {
                var titleText = obj as BGText;
                titleText.text = "？？？";
                titleText.SetTextAlpha(0.5f);
                titleText.SetBGEnable(false);
            }
        });
    }
}
