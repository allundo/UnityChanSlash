using UnityEngine;
using UniRx;

public class TitleSceneMediator : SceneMediator
{
    [SerializeField] TitleUIHandler titleUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(Logo, titleUIHandler.SkipLogo);
    }

    private void Logo()
    {
        titleUIHandler.Logo()
            .SelectMany(_ => sceneLoader.LoadSceneAsync(1, 3f))
            .IgnoreElements()
            .Subscribe(null, titleUIHandler.ToTitle)
            .AddTo(this);

        titleUIHandler.TransitSignal
            .Subscribe(_ => sceneLoader.SceneTransition())
            .AddTo(this);
    }
}
