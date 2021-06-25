using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleUIHandler : MonoBehaviour
{
    [SerializeField] private LogoAnimation logo = default;
    [SerializeField] private StartButton unityChanIcon = default;
    [SerializeField] private CameraWork cameraWork = default;
    [SerializeField] private TitleAnimation txtUnity = default;
    [SerializeField] private TitleAnimation txtSlash = default;

    private AsyncOperation asyncLoad;

    void Start()
    {
        unityChanIcon.startButton.onClick.AddListener(SceneTransition);
        unityChanIcon.startButton.onClick.AddListener(cameraWork.StopCameraWork);

        unityChanIcon.LogoTween();
        logo.LogoTween(() => StartCoroutine(LoadScene()));
    }

    private void ToTitle()
    {
        cameraWork.ToTitle(cameraWork.StartCameraWork);

        unityChanIcon.ToTitle();
        logo.ToTitle();

        txtUnity.TitleTween();
        txtSlash.TitleTween();
    }

    private IEnumerator LoadScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync(1);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return new WaitForSeconds(3);
        }

        ToTitle();
    }

    private void SceneTransition()
    {
        asyncLoad.allowSceneActivation = true;
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        SceneManager.UnloadSceneAsync(0);
    }
}
