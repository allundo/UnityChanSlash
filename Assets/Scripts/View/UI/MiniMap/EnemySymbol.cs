using UnityEngine;
using DG.Tweening;

public class EnemySymbol : UISymbol
{
    protected FadeTweenImage fade;
    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        fade = new FadeTweenImage(gameObject, 1f);
    }

    public override void Activate()
    {
        fade.In(0.5f, 0f, base.Activate).Play();
    }
    public override void Inactivate()
    {
        fade.Out(0.5f, 0f, null, base.Inactivate).Play();
    }
}