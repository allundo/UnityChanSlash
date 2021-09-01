using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
public class BaseSceneManager : MonoBehaviour
{
    protected SceneLoader sceneLoader;

    protected virtual void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
    }
}