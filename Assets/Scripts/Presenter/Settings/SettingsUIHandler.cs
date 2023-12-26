using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Linq;

public class SettingsUIHandler : MonoBehaviour
{
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private AlphaSlider[] alphaSetters = default;
    [SerializeField] private Button toTitleBtn = default;
    [SerializeField] private Button deleteSaveBtn = default;

    public IObservable<DataStoreAgent.SettingData> TransitSignal { get; private set; }

    void Awake()
    {
        TransitSignal = toTitleBtn
            .OnClickAsObservable().First() // ContinueWith() cannot handle duplicated click events
            .ContinueWith(_ => fade.FadeOutObservable(2f))
            .ContinueWith(_ => Observable.Return(RetrieveSettings()));

        deleteSaveBtn.OnClickAsObservable().First()
            .Subscribe(_ =>
            {
                DataStoreAgent.Instance.DeleteAllDataFiles();
                deleteSaveBtn.enabled = false;
            })
            .AddTo(this);
    }

    void Start()
    {
        alphaSetters[0].SetDisplay(false);

        foreach (UIType type in Enum.GetValues(typeof(UIType)))
        {
            if (type == UIType.None) continue;
            alphaSetters[(int)type].SetTypeName(DataStoreAgent.SettingData.Label(type));
        }

        SetInteractable(false);
    }

    public void ShowUIs()
    {
        fade.color = Color.black;
        fade.FadeInObservable(2f)
            .Subscribe(_ => SetInteractable(true))
            .AddTo(this);
    }

    public void ApplySettings(DataStoreAgent.SettingData data)
    {

        foreach (UIType type in Enum.GetValues(typeof(UIType)))
        {
            alphaSetters[(int)type].SetAlpha(data[type]);
        }
    }

    private void SetInteractable(bool isEnable)
    {
        alphaSetters.ForEach(setter => setter.SetInteractable(isEnable));
    }

    private DataStoreAgent.SettingData RetrieveSettings()
    {
        return new DataStoreAgent.SettingData(alphaSetters.Select(setter => setter.GetAlpha()).ToArray());
    }
}