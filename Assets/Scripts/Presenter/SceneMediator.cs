using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;

public abstract class SceneMediator : MonoBehaviour
{
    protected SceneLoader sceneLoader;

    private Action[] startActions;

    protected virtual void Awake()
    {
        LoadSingleton<ResourceLoader>("Prefabs/System/ResourceLoader");
        LoadSingleton<GameInfo>("Prefabs/System/GameInfo");
        LoadSingleton<DataStoreAgent>("Prefabs/System/DataStoreAgent");

        sceneLoader = new SceneLoader();
    }

    protected void LoadSingleton<T>(string prefabFilePath) where T : MonoBehaviour
    {
        if (FindObjectOfType(typeof(T)) == null)
        {
            Instantiate(Resources.Load<T>(prefabFilePath)); ;
        }
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

    protected void SceneTransition(Action updateGameInfo)
    {
        updateGameInfo();
        sceneLoader.SceneTransition();
    }

    /// <summary>
    /// Selects scene starting method of next scene by ID and apply the scene transition.
    /// </summary>
    /// <param name="startActionID">Index of start actions of next scene manager regestered by SetStartActions().</param>
    protected void SceneTransition(int startActionID)
        => SceneTransition(() => GameInfo.Instance.startActionID = startActionID);

    /// <summary>
    /// Selects scene starting method of next scene by ID and apply the scene transition.
    /// </summary>
    /// <param name="startActionID">Index of start actions of next scene manager regestered by SetStartActions().</param>
    /// <param name="updateGameInfo">Callback action that updates GameInfo parameters before the scene transition.</param>
    protected void SceneTransition(int startActionID, Action updateGameInfo)
    {
        updateGameInfo();
        SceneTransition(startActionID);
    }

    protected void ForceTransitScene(int sceneBuildIndex, int startActionID)
    {
        GameInfo.Instance.startActionID = startActionID;
        DOTween.KillAll();
        SceneManager.LoadScene(sceneBuildIndex);

#if UNITY_EDITOR
        GameInfo.Instance.isScenePlayedByEditor = false;
#endif
    }

    /// <summary>
    /// Loads next scene without waiting and transit immediately after the loading.
    /// </summary>
    /// <param name="sceneBuildIndex">0 origin scenes list index number registered in editor "Build Settings".</param>
    /// <param name="startActionID">Index of start actions of next scene manager regestered by SetStartActions().</param>
    protected void LoadSceneAndTransit(int sceneBuildIndex, int startActionID)
    {
        GameInfo.Instance.startActionID = startActionID;
        sceneLoader.StartLoadScene(sceneBuildIndex, true);
    }
}
