using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
public class BaseSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject prefabGameInfo = default;
    protected SceneLoader sceneLoader;

    protected virtual void Awake()
    {
        if (FindObjectOfType(typeof(GameInfo)) == null)
        {
            Instantiate(prefabGameInfo, Vector3.zero, Quaternion.identity); ;
            Debug.Log("GameInfo not found. Instantiated temporary.");
        }
        sceneLoader = GetComponent<SceneLoader>();
    }
}
