using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : SwitchingContentBase
{
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

    protected override void Awake()
    {
        base.Awake();
        isShown = false;
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

    protected override void EnableUI() => gameObject.SetActive(true);
    protected override void DisableUI() => gameObject.SetActive(false);
}
