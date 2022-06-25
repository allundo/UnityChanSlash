using System;
using UnityEngine;

public abstract class SingletonComponent<T> : MonoBehaviour where T : class
{
    [SerializeField] private bool isPersistent = false;

    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfInterface();
                if (instance == null)
                {
                    Debug.LogError(typeof(T) + " を実装したコンポーネントが見つかりません");
                }
            }

            return instance;
        }
    }

    private static T FindObjectOfInterface()
    {
        foreach (var component in GameObject.FindObjectsOfType<Component>())
        {
            if (component is T) return component as T;
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
