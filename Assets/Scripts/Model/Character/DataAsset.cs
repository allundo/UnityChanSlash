using UnityEngine;
using System;

public class DataAsset<T> : ScriptableObject
{
    [SerializeField] protected T[] setParams;
    public T Param(int index)
    {
        if (index >= setParams.Length) throw new IndexOutOfRangeException("Parameter number " + index + "is not found.");

        return setParams[index];
    }
}

// System.SerializeField属性を使用することで、Inspector上で変更した値がアセットに保存されるようになります
[System.Serializable]
public class MobParam
{
    [SerializeField] public string name = "キャラクター";

    [SerializeField] public float defaultLifeMax = 10;

    [SerializeField] public bool isOnGround = true;

    [SerializeField] public float attack = 1.0f;

    [SerializeField] public float shield = 0.0f;

    [SerializeField] public float faceDamageMultiplier = 1.0f;

    [SerializeField] public float sideDamageMultiplier = 1.5f;

    [SerializeField] public float backDamageMultiplier = 2.0f;

    [SerializeField] public float restDamageMultiplier = 6.0f;

    [SerializeField] public float armorMultiplier = 1.0f;
}

[System.Serializable]
public class ItemSource
{
    [SerializeField] public string name = "種別";
    [SerializeField] public Material material = default;
    [SerializeField] public ParticleSystem vfx = default;
    [SerializeField] public AudioSource sfx = default;
    [SerializeField] public float duration = 0.2f;
}
