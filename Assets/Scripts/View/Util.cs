using System;
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
        => UnityEngine.Random.Range(0, bottom) < top;


    public static T ConvertTo<T>(int index) where T : Enum
        => (T)Enum.ToObject(typeof(T), index);

    public static int Count<T>() where T : Enum => Enum.GetNames(typeof(T)).Length;

    public static T[] GetValues<T>() where T : Enum => (T[])Enum.GetValues(typeof(T));

    public static int GetEnemyLevel(uint range = 5)
    {
        int min = -(int)(range / 2);
        return Mathf.Max(0, GameInfo.Instance.currentFloor + UnityEngine.Random.Range(min, min + (int)range));
    }
    public static string TimeFormat(int sec)
    {
        int min = sec / 60;
        int hour = min / 60;
        return $"{hour,3:D}:{min % 60:00}:{sec % 60:00}";
    }

    public static string PercentFormat(ulong rate_x1000) => $"{rate_x1000 / 10,3:D}.{rate_x1000 % 10}";
    public static string PercentFormat(float rate) => PercentFormat((ulong)(rate * 1000f));
}
