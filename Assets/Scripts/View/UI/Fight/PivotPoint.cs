using UnityEngine;

public class PivotPoint : TargetingUI
{
    protected override void OnShow(Vector2 pos) { }
    protected override void OnActive(Vector2 pos)
    {
        rectTransform.position = pos;
    }
}