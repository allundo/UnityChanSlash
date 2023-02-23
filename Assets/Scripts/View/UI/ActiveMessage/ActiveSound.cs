using UnityEngine;

public enum ActiveSoundType
{
    None = -1,
    LevelUp = 0,
    WeaponEquip,
    WeaponBreak,
    KeyLockOpen,
}

public class ActiveSound : MonoBehaviour
{
    [SerializeField] private AudioSource[] snds = default;

    public void Play(ActiveSoundType type)
    {
        if (type != ActiveSoundType.None) snds[(int)type].PlayEx();
    }
}
