using UnityEngine;
using UniRx;

public class SettingsSceneMediator : SceneMediator
{
    [SerializeField] private SettingsUIHandler settingUIHandler = default;

    protected override void InitBeforeStart()
    {
        SetStartActions(DisplaySettings, DebugSettings);

        sceneLoader.StartLoadScene(0);

        Time.timeScale = 1f;

#if UNITY_EDITOR
        GameInfo.Instance.SetDebugAction(1);
#endif
    }

    private void DisplaySettings()
    {
        var dataStoreAgent = DataStoreAgent.Instance;

        settingUIHandler.TransitSignal
            .Subscribe(data =>
            {
                dataStoreAgent.SaveSettings(data);
                SceneTransition(0); // TitleSceneMediator.Logo()
            })
            .AddTo(this);

        settingUIHandler.ApplySettings(dataStoreAgent.LoadSettingData());
        settingUIHandler.ShowUIs();
    }

    private void DebugSettings()
    {
        Debug.Log("DEBUG MODE");

        settingUIHandler.TransitSignal
            .Subscribe(_ => SceneTransition(0))
            .AddTo(this);

        settingUIHandler.ApplySettings(new DataStoreAgent.SettingData());
        settingUIHandler.ShowUIs();
    }
}
