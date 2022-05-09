using UnityEngine;
using System;

public abstract class SceneMediator : MonoBehaviour
{
    [SerializeField] private GameObject prefabGameInfo = default;
    protected SceneLoader sceneLoader;

    private Action[] startActions;

    protected virtual void Awake()
    {
        if (FindObjectOfType(typeof(GameInfo)) == null)
        {
            Instantiate(prefabGameInfo, Vector3.zero, Quaternion.identity); ;
        }
        sceneLoader = new SceneLoader();
    }

    private void Start()
    {
        InitBeforeStart();
        startActions[GameInfo.Instance.startActionID]();
    }

    /// <summary>
    /// This method called at Start() statement before calling startAction. <br />
    /// SetStartActions() must be called in this method.
    /// </summary>
    protected abstract void InitBeforeStart();

    protected void SetStartActions(params Action[] startActions)
    {
        this.startActions = startActions;
    }

    protected void SceneTransition(Action updateGameInfo = null)
    {
        (updateGameInfo ?? (() => { }))();
        sceneLoader.SceneTransition();
    }

    protected void SceneTransition(int startActionID)
        => SceneTransition(() => GameInfo.Instance.startActionID = startActionID);

    protected void SceneTransition(int startActionID, Action updateGameInfo)
    {
        updateGameInfo();
        SceneTransition(startActionID);
    }
}
