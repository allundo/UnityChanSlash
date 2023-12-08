using UnityEngine;
using TMPro;

public class StatusUI : SwitchingContentBase
{
    [SerializeField] private TextMeshProUGUI unityChan = default;
    [SerializeField] private StatusContent level = default;
    [SerializeField] private StatusContent attack = default;
    [SerializeField] private StatusContent defense = default;
    [SerializeField] private StatusContent magic = default;
    [SerializeField] private StatusContent resist = default;
    [SerializeField] private StatusTextHidden levelGainType = default;

    private float expToNextLevel = 10000f;

    private float landscapeRatio;
    private IStatusContent[] contents;

    protected override void Awake()
    {
        base.Awake();
        unityChan.enabled = false;
        isShown = false;
        landscapeRatio = landscapeSize / portraitSize;
        contents = new IStatusContent[] { level, attack, defense, magic, resist, levelGainType };
    }
    public override void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                SetSize(portraitSize);
                SetPortraitPos();
                contents.ForEach(content => content.SetSize(1f));
                break;

            case DeviceOrientation.LandscapeRight:
                SetSize(landscapeSize);
                SetLandscapePos();
                contents.ForEach(content => content.SetSize(landscapeRatio));
                break;
        }
    }
    public void UpdateValues(DispStatus status)
    {
        level.SetValue(status.level);
        expToNextLevel = status.expToNextLevel;
        level.SetSubValues(status.exp / expToNextLevel, expToNextLevel - status.exp);
        levelGainType.SetValue(status.levelGainTypeName);
        attack.SetValue(status.attack);
        attack.SetSubValues(status.equipR, status.equipL);
        defense.SetValue(status.armor);
        defense.SetSubValues(status.shield);
        magic.SetValue(status.magic);
        resist.SetSubValues(status.resistFire, status.resistIce, status.resistDark, status.resistLight);
    }

    public void UpdateExp(float exp)
    {
        level.SetSubValues(exp / expToNextLevel, expToNextLevel - exp);
    }

    public void UpdateShield(float shield)
    {
        defense.SetSubValues(shield);
    }

    public override void SetEnable(bool isEnabled) => gameObject.SetActive(isEnabled);

    public override void ExpandUI()
    {
        unityChan.enabled = true;
        ResetUISize(expandSize);
        rectTransform.anchoredPosition = anchoredCenter;
        contents.ForEach(content => content.Expand());
    }

    public override void ShrinkUI()
    {
        unityChan.enabled = false;
        ResetUISize(currentSize);
        rectTransform.anchoredPosition = currentPos;
        contents.ForEach(content => content.Shrink());
    }

    protected void ResetUISize(float size)
    {
        rectTransform.sizeDelta = new Vector2(size, size);
    }
}
