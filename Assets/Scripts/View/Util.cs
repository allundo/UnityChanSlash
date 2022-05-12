using UnityEngine;
using UnityEngine.UI;

public class Util
{
    public static bool IsNull<T>(T prefab) where T : UnityEngine.Object
    {
        if (prefab == null)
        {
#if UNITY_EDITOR
            Debug.Log("Tried to instantiate null object.");
#endif
            return true;
        }

        return false;
    }
    public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : UnityEngine.Object
        => IsNull(prefab) ? prefab : UnityEngine.Object.Instantiate(prefab, position, rotation, parent);

    public static T Instantiate<T>(T prefab, Transform parent = null, bool instantiateInWorldSpace = false) where T : UnityEngine.Object
        => IsNull(prefab) ? prefab : UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace);

    public static Material SwitchMaterial(Renderer renderer, Material matSrc)
    {
        var matPrev = renderer.sharedMaterial;
        renderer.material = new Material(matSrc);
        if (matPrev != null) UnityEngine.Object.Destroy(matPrev);
        return renderer.sharedMaterial;
    }

    public static Material SwitchMaterial(MaskableGraphic image, Material matSrc)
    {
        var matPrev = image.material;
        if (matPrev != null) UnityEngine.Object.Destroy(matPrev);
        return image.material = new Material(matSrc);
    }

    /// <summary>
    /// Simple judgement with probability: 1 / elems
    /// </summary>
    /// <param name="elems">denominator that determines probability</param>
    /// <returns>true if succeed</returns>
    public static bool Judge(int elems) => Util.DiceRoll(1, elems);

    /// <summary>
    /// Simple judgement with probability: top / bottom
    /// </summary>
    /// <param name="top">numerator that determines probability</param>
    /// <param name="bottom">denominator that determines probability</param>
    /// <returns>true if succeed</returns>
    public static bool DiceRoll(int top, int bottom)
        => Random.Range(0, bottom) < top;

}
