using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using DG.Tweening;

public class SettingsUIHandler : MonoBehaviour
{
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private AlphaSlider bgSetter = default;
    [SerializeField] private AlphaSlider attackButtonSetter = default;
    [SerializeField] private AlphaSlider buttonRegionSetter = default;
    [SerializeField] private AlphaSlider enemyGaugeSetter = default;
    [SerializeField] private AlphaSlider moveButtonSetter = default;
    [SerializeField] private AlphaSlider handleButtonSetter = default;
    [SerializeField] private Button toTitleBtn = default;

    public IObservable<DataStoreAgent.SettingData> TransitSignal { get; private set; }


    void Awake()
    {
        TransitSignal = toTitleBtn
            .OnClickAsObservable().First() // ContinueWith() cannot handle duplicated click events
            .ContinueWith(_ => fade.FadeOutObservable(2f))
            .ContinueWith(_ => Observable.Return(RetrieveSettings()));
    }

    void Start()
    {
        bgSetter.SetDisplay(false);
        bgSetter.SetAlpha(1f);

        enemyGaugeSetter.SetTypeName("敵体力ゲージ");
        attackButtonSetter.SetTypeName("攻撃種別アイコン");
        buttonRegionSetter.SetTypeName("攻撃種別領域");
        moveButtonSetter.SetTypeName("移動・ガードボタン");
        handleButtonSetter.SetTypeName("操作ボタン");

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
        enemyGaugeSetter.SetAlpha(data.fightCircleAlpha);
        attackButtonSetter.SetAlpha(data.attackButtonAlpha);
        buttonRegionSetter.SetAlpha(data.buttonRegionAlpha);
        moveButtonSetter.SetAlpha(data.moveButtonAlpha);
        handleButtonSetter.SetAlpha(data.handleButtonAlpha);
    }

    private void SetInteractable(bool isEnable)
    {
        new AlphaSlider[] { bgSetter, attackButtonSetter, buttonRegionSetter, enemyGaugeSetter, moveButtonSetter, handleButtonSetter }
            .ForEach(setter => setter.SetInteractable(isEnable));
    }

    private DataStoreAgent.SettingData RetrieveSettings()
    {
        return new DataStoreAgent.SettingData()
        {
            fightCircleAlpha = enemyGaugeSetter.GetAlpha(),
            attackButtonAlpha = attackButtonSetter.GetAlpha(),
            buttonRegionAlpha = buttonRegionSetter.GetAlpha(),
            moveButtonAlpha = moveButtonSetter.GetAlpha(),
            handleButtonAlpha = handleButtonSetter.GetAlpha(),
        };
    }
}