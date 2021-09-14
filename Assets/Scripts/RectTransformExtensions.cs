using UnityEngine;

static public class RectTransformExtensions
{
    static public Vector2 GetScreenPos(this RectTransform rt)
    {
        var parentRT = rt.parent?.GetComponent<RectTransform>();

        if (parentRT == null)
        {
            return (Vector2)rt.position;
        }

        return (Vector2)rt.localPosition + parentRT.GetScreenPos();
    }
}
