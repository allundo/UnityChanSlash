using UnityEngine;
using UniRx;

[RequireComponent(typeof(SceneLoader))]
public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    protected SceneLoader sceneLoader;

    void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
    }

    void Start()
    {
        titleUIHandler.Logo()
            .IgnoreElements()
            .Subscribe(null, StartLoading)
            .AddTo(this);

        titleUIHandler.TransitSignal
            .Subscribe(_ => sceneLoader.SceneTransition())
            .AddTo(this);
    }

    private void StartLoading()
    {
        sceneLoader.LoadSceneAsync(1, 3f)
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.ToTitle)
            .AddTo(this);
    }
}