using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class TitleUIHandler : MonoBehaviour
{
    [SerializeField] private LogoAnimation logo = default;
    [SerializeField] private StartButton unityChanIcon = default;
    [SerializeField] private Camera mainCamera = default;
    [SerializeField] private TitleAnimation txtUnity = default;
    [SerializeField] private TitleAnimation txtSlash = default;

    private AsyncOperation asyncLoad;

    void Awake()
    {
        mainCamera.transform.position = new Vector3(-10f, 1f, 2.5f);
    }

    void Start()
    {
        unityChanIcon.startButton.onClick.AddListener(SceneTransition);

        unityChanIcon.LogoTween();
        logo.LogoTween(() => StartCoroutine(LoadScene()));
    }

    private void ToTitle()
    {
        mainCamera.transform.DOMove(new Vector3(0f, 1f, 2.5f), 1.2f).Play();

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
