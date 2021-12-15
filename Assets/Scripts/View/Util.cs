using UnityEngine;

public class Util
{
    public static bool IsNull<T>(T prefab)
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
}
