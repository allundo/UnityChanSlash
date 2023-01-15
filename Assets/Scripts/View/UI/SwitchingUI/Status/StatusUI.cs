using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : SwitchingContentBase
{
    [SerializeField] private StatusContent level = default;
    [SerializeField] private StatusContent attack = default;
    [SerializeField] private StatusContent defense = default;
    [SerializeField] private StatusContent magic = default;
    [SerializeField] private StatusContent resist = default;

    private float expToNextLevel = 10000f;

    private float landscapeRatio;
    private IStatusContent[] contents;

    protected override void Awake()
    {
        base.Awake();
        isShown = false;
        landscapeRatio = landscapeSize / portraitSize;
        contents = new IStatusContent[] { level, attack, defense, magic, resist };
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
        level.SetSubValues(status.exp / expToNextLevel);
        attack.SetValue(status.attack);
        attack.SetSubValues(status.equipR, status.equipL);
        defense.SetValue(status.armor);
        defense.SetSubValues(status.shield);
        magic.SetValue(status.magic);
        resist.SetSubValues(status.resistFire, status.resistIce, status.resistDark, status.resistLight);
    }

    public void UpdateExp(float exp)
    {
        level.SetSubValues(exp / expToNextLevel);
    }

    public void UpdateShield(float shield)
    {
        defense.SetSubValues(shield);
    }

    public override void SetEnable(bool isEnabled) => gameObject.SetActive(isEnabled);
}
