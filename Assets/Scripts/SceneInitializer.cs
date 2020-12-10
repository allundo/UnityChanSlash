using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public abstract class SceneInitializer
{
    static Stack<Scene> pastScenes = new Stack<Scene>();
    public bool IsReady => aop != null && aop.isDone;

    private AsyncOperation aop = null;
    public virtual void LoadSceneAsync(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
    }



}