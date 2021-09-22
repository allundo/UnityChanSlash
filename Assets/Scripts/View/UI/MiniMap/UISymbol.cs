using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(MaskableGraphic))]
public class UISymbol : SpawnObject<UISymbol>
{
    protected RectTransform rectTransform;
    protected MaskableGraphic image;
    protected Vector2 parentPos;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentPos = transform.parent.GetComponent<RectTransform>().localPosition;
        image = GetComponent<MaskableGraphic>();
    }

    public override UISymbol OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPos(new Vector2(pos.x, pos.y));
        Activate();
        return this;
    }

    public virtual UISymbol SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
        return this;
    }

    public virtual UISymbol SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
        return this;
    }
}