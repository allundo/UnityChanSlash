using UnityEngine;
using UniRx;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, SkipLogo);

        titleUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0))
            .AddTo(this);
    }

    private void Logo()
    {
        titleUIHandler.Logo()
            .SelectMany(_ => sceneLoader.LoadSceneAsync(1, 3f))
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.ToTitle)
            .AddTo(this);
    }

    private void SkipLogo()
    {
        sceneLoader.LoadSceneAsync(1)
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.SkipLogo)
            .AddTo(this);
    }
}
