using UnityEngine;
using System;
using UniRx;

public class ActiveMessageController : SingletonMonoBehaviour<ActiveMessageController>
{
    [SerializeField] protected ActiveMessageBox messageBox = default;
    [SerializeField] protected SDIcon sdIcon = default;
    [SerializeField] protected ActiveSound activeSound = default;

    protected RectTransform rectTransform;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        Observable.Merge(messageBox.CloseSignal, sdIcon.CloseSignal)
            .Subscribe(_ => Close())
            .AddTo(this);
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rectTransform.anchoredPosition = new Vector2(0f, -860f);
                break;

            case DeviceOrientation.LandscapeRight:
                rectTransform.anchoredPosition = new Vector2(0f, -440f);
                break;
        }
        sdIcon.ResetOrientation(orientation);
    }

    public void Close()
    {
        sdIcon.Inactivate();
        messageBox.Inactivate();
    }

    public void InputMessageData(string message, SDFaceID face = SDFaceID.DEFAULT, SDEmotionID emotion = SDEmotionID.NONE, ActiveSoundType type = ActiveSoundType.None)
        => InputMessageData(new ActiveMessageData(message, face, emotion), type);

    public void InputMessageData(ActiveMessageData messageData, ActiveSoundType type = ActiveSoundType.None)
    {
        sdIcon.Activate(messageData);
        messageBox.Activate(messageData);
        activeSound.Play(type);
    }

    public void InputMessageWithDelay(ActiveMessageData messageData, float delay = 1f)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => InputMessageData(messageData))
            .AddTo(this);
    }

    public void LevelUp(int level) => InputMessageData($"レベル {level + 1} に上がった！", SDFaceID.SMILE, SDEmotionID.WAIWAI, ActiveSoundType.LevelUp);

    public void Equip(string name) => InputMessageData(new ActiveMessageData($"{name} を装備した！"), ActiveSoundType.WeaponEquip);

    public void BreakItem(string name) => InputMessageData($"{name} は壊れてしまった！", SDFaceID.SAD2, SDEmotionID.CONFUSE, ActiveSoundType.WeaponBreak);

    public void ClassChange(string name) => InputMessageWithDelay(new ActiveMessageData($"{name} のクラスに変化した！", SDFaceID.SURPRISE, SDEmotionID.SURPRISE));

    public void GetItem(ItemInfo item) => InputMessageData(item.name + " を手に入れた！", SDFaceID.SMILE, SDEmotionID.WAIWAI);

    public void TameSucceeded(IEnemyStatus status) => InputMessageData(status.Name + " を懐柔した！", SDFaceID.SMILE, SDEmotionID.WAIWAI);

    public void AlreadyTamed(IEnemyStatus status) => InputMessageData(status.Name + " はすでに懐柔されている…！", SDFaceID.SURPRISE, SDEmotionID.EXQUESTION);

    public void InspectTile(ITile tile)
    {
        if (tile is Pit)
        {
            if ((tile as Pit).IsOpen)
            {
                InputMessageData("落とし穴がある。", SDFaceID.DISATTRACT, SDEmotionID.BLUE);
            }
            else
            {
                InputMessageData("落とし穴はっけん！", SDFaceID.ANGRY2, SDEmotionID.SURPRISE);
            }
        }
        else if (tile is Furniture)
        {
            InputMessageData("木製の家具がある。", SDFaceID.SURPRISE, SDEmotionID.QUESTION);
        }
        else
        {
            InputMessageData("なにもない");
        }
    }

    public void KeyLockOpen() => InputMessageData("扉の鍵が開いた！", SDFaceID.SMILE, SDEmotionID.WAIWAI, ActiveSoundType.KeyLockOpen);
}
