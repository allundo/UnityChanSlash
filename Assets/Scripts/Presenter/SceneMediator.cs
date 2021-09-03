using UnityEngine;
using System;

[RequireComponent(typeof(SceneLoader))]
public abstract class SceneMediator : MonoBehaviour
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

    protected void SceneTransition(Action updateGameInfo = null)
    {
        (updateGameInfo ?? (() => { }))();
        sceneLoader.SceneTransition();
    }

    protected void SceneTransition(int startActionID)
        => SceneTransition(() => GameInfo.Instance.startActionID = startActionID);
}
