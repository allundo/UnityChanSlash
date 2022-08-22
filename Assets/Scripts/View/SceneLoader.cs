using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading;
using UniRx;
using DG.Tweening;

public class SceneLoader
{
    private AsyncOperation asyncLoad;
    private int currentLoading = -1;

    /// <summary>
    /// Starts loading next scene in parallel. To transit to next scene, it's needed to call SceneTransition().
    /// </summary>
    /// <param name="sceneBuildIndex">0 origin scenes list index number registered in editor "Build Settings".</param>
    /// <param name="waitSec">Waiting unit time[sec]. Waiting is repeated until scene loading has finished.</param>
    /// <returns></returns>
    public IObservable<Unit> LoadSceneAsync(int sceneBuildIndex, float waitSec = 0f)
        => Observable.FromCoroutine(token => LoadScene(token, sceneBuildIndex, waitSec));

    private IEnumerator LoadScene(CancellationToken token, int sceneBuildIndex, float waitSec = 0f)
    {
        StartLoadScene(sceneBuildIndex);

        var waitFor = WaitFor(waitSec);

        while (asyncLoad.progress < 0.9f)
        {
            yield return waitFor;
        }

        if (token.IsCancellationRequested) UnloadCurrentLoadingScene();
    }

    /// <summary>
    /// Loads next scene without waiting.
    /// </summary>
    /// <param name="sceneBuildIndex">0 origin scenes list index number registered in editor "Build Settings".</param>
    /// <param name="allowSceneActivation">Scene transition happens immediately after the loading is finished if TRUE.</param>
    public void StartLoadScene(int sceneBuildIndex, bool allowSceneActivation = false)
    {
        if (currentLoading != sceneBuildIndex)
        {
            currentLoading = sceneBuildIndex;
            asyncLoad = SceneManager.LoadSceneAsync(sceneBuildIndex);
        }

        if (allowSceneActivation)
        {
            SceneTransition();
            return;
        }

        asyncLoad.allowSceneActivation = false;
    }

    public void UnloadCurrentLoadingScene()
    {
        if (currentLoading == -1) return;

        // SceneManager can unload only activated scene.
        DOTween.KillAll();
        asyncLoad.allowSceneActivation = true;

        asyncLoad = SceneManager.UnloadSceneAsync(currentLoading);
        currentLoading = -1;
    }

    /// <summary>
    /// Returns loading wait yield instruction with specified time[sec]. If wait time < 0.02f, wait until next frame.
    /// </summary>
    private YieldInstruction WaitFor(float waitSec)
        => waitSec < 0.02f ?
            new WaitForEndOfFrame() as YieldInstruction
            : new WaitForSeconds(waitSec) as YieldInstruction;

    /// <summary>
    /// Activate a preloaded scene.
    /// </summary>
    public void SceneTransition()
    {
        DOTween.KillAll();
        asyncLoad.allowSceneActivation = true;
        currentLoading = -1;

#if UNITY_EDITOR
        GameInfo.Instance.isScenePlayedByEditor = false;
#endif

    }
}
