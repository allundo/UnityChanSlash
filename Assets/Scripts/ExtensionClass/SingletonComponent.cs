using System;
using UnityEngine;

public class SingletonComponent<T> : MonoBehaviour where T : class
{
    [SerializeField] private bool isPersistent = false;

    private static T instance;
    public static T Instance
    {
        get
        {
            // Type of T(class) cannot be compared with {null} GameObject
            var component = instance as MonoBehaviour;

            if (component == null)
            {
                // Type of T(class) cannot be compared with {null} GameObject
                component = FindObjectOfInterface() as MonoBehaviour;

                if (component == null)
                {
                    Debug.LogError(typeof(T) + " を実装したコンポーネントが見つかりません");
                }
            }

            return component as T;
        }
    }

    private static T FindObjectOfInterface()
    {
        foreach (var component in GameObject.FindObjectsOfType<Component>())
        {
            if (component is T && component is MonoBehaviour) return component as T;
        }
        return null;
    }

    protected virtual void Awake()
    {
        var obj = Instance;

        if (!(obj is MonoBehaviour))
        {
            var typeName = typeof(T).ToString();
            throw new TypeInitializationException(typeName, new Exception(typeName + " は MonoBehaviour を継承していません"));
        }

        var component = obj as MonoBehaviour;

        if (this != component)
        {
            Destroy(this);
            Debug.LogError(
                typeof(T) +
                " は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました." +
                " アタッチされているGameObjectは " + component.gameObject.name + " です.");
            return;
        }

        if (isPersistent) DontDestroyOnLoad(this.gameObject);
    }
}