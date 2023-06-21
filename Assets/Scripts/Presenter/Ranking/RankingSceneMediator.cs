using UnityEngine;
using UniRx;

public class RankingSceneMediator : SceneMediator
{
    [SerializeField] private RankingUIHandler rankingUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(ShowRanking, DebugShowRanking);

        sceneLoader.StartLoadScene(0);

        Time.timeScale = 1f;

        GameInfo gameInfo = GameInfo.Instance;

        rankingUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(1))
            .AddTo(this);

#if UNITY_EDITOR
        if (gameInfo.isScenePlayedByEditor) gameInfo.startActionID = 1;
#endif

    }

    private void ShowRanking()
    {
        rankingUIHandler.ViewInfo();
    }

    private void DebugShowRanking()
    {
        Debug.Log("DEBUG MODE");
        rankingUIHandler.ViewInfo();
    }
}
