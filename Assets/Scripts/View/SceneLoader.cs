using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UniRx;

public class SceneLoader : MonoBehaviour
{
    private AsyncOperation asyncLoad;

    public IObservable<Unit> LoadSceneAsync(int sceneBuildIndex, float waitSec = 0f)
        => Observable.FromCoroutine(() => LoadScene(sceneBuildIndex, waitSec));

    private IEnumerator LoadScene(int sceneBuildIndex, float waitSec = 0f)
    {
        asyncLoad = SceneManager.LoadSceneAsync(sceneBuildIndex);
        asyncLoad.allowSceneActivation = false;

        var waitFor = WaitFor(waitSec);

        while (asyncLoad.progress < 0.9f)
        {
            yield return waitFor;
        }
    }

    /// <summary>
    /// Returns loading wait yield instruction with specified time[sec]. If wait time < 0.02f, wait until next frame.
    /// </summary>
    private YieldInstruction WaitFor(float waitSec)
        => waitSec < 0.02f ?
            new WaitForEndOfFrame() as YieldInstruction
            : new WaitForSeconds(waitSec) as YieldInstruction;

    public void SceneTransition()
    {
        asyncLoad.allowSceneActivation = true;
    }
}