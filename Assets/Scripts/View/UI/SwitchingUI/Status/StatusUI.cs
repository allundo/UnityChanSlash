using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;
using DG.Tweening;

public class StatusUI : SwitchingUIBase
{
    [SerializeField] private Button mapBtn = default;
    [SerializeField] private TextMeshProUGUI level = default;
    [SerializeField] private Image exp = default;
    [SerializeField] private TextMeshProUGUI attack = default;
    [SerializeField] private TextMeshProUGUI equipR = default;
    [SerializeField] private TextMeshProUGUI equipL = default;
    [SerializeField] private TextMeshProUGUI armor = default;
    [SerializeField] private TextMeshProUGUI shield = default;
    [SerializeField] private TextMeshProUGUI magic = default;
    [SerializeField] private TextMeshProUGUI resistFire = default;
    [SerializeField] private TextMeshProUGUI resistIce = default;
    [SerializeField] private TextMeshProUGUI resistDark = default;
    [SerializeField] private TextMeshProUGUI resistLight = default;

    private float expToNextLevel = 10000f;

    public IObservable<Unit> Switch => mapBtn.OnClickAsObservable();

    protected override void Awake()
    {
        base.Awake();
        isShown = false;
    }

    void Start()
    {
        Switch.Subscribe(_ => HideStatus()).AddTo(this);
    }

    public void UpdateValues(DispStatus status)
    {
        level.text = status.level.ToString();
        expToNextLevel = status.expToNextLevel;
        exp.fillAmount = status.exp / expToNextLevel;
        attack.text = status.attack.ToString();
        equipR.text = status.equipR.ToString();
        equipL.text = status.equipL.ToString();
        armor.text = status.armor.ToString();
        shield.text = status.shield.ToString();
        magic.text = status.magic.ToString();
        resistFire.text = status.resistFire.ToString();
        resistIce.text = status.resistIce.ToString();
        resistDark.text = status.resistDark.ToString();
        resistLight.text = status.resistLight.ToString();
    }

    public void UpdateExp(float exp)
    {
        this.exp.fillAmount = exp / expToNextLevel;
    }

    public void UpdateShield(float shield)
    {
        this.shield.text = Mathf.RoundToInt(shield).ToString();
    }

    private void HideStatus(float duration = 0.25f)
    {
        mapBtn.enabled = false;
        mapBtn.gameObject.SetActive(false);
        SwitchUI(duration, () => gameObject.SetActive(false));
    }

    public void ShowStatus(float duration = 0.25f, float delay = 0.25f)
    {
        gameObject.SetActive(true);
        mapBtn.gameObject.SetActive(true);
        DOVirtual.DelayedCall(delay, () => SwitchUI(duration, () => mapBtn.enabled = true)).Play();
    }
}
