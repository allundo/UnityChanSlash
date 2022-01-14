using UnityEngine;
using System;

public class DataAsset<T> : ScriptableObject
{
    [SerializeField] protected T[] setParams;

    public int Length => setParams.Length;

    public T Param(int index)
    {
        if (index >= setParams.Length) throw new IndexOutOfRangeException("Parameter number " + index + "is not found.");

        return setParams[index];
    }

    public void ForEach(Action<T> action)
    {
        setParams.ForEach(param => action(param));
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

    [SerializeField] public MobStatus prefab = default;
}

[System.Serializable]
public class EnemyParam : MobParam
{
    [SerializeField] public EnemyType type = EnemyType.None;
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

[System.Serializable]
public class DamageSndSource
{
    [SerializeField] public string name = "攻撃タイプ";
    [SerializeField] public AudioSource damage = default;
    [SerializeField] public AudioSource critical = default;
    [SerializeField] public AudioSource guard = default;
}

[System.Serializable]
public class EnemyTypesSource
{
    [SerializeField] public EnemyType[] types;
}

[System.Serializable]
public class FloorMaterialsSource
{
    [SerializeField] public string name = "通常ダンジョン";
    [SerializeField] public Material ground = default;
    [SerializeField] public Material wall = default;
    [SerializeField] public Material gate = default;
    [SerializeField] public Material door = default;
    [SerializeField] public Material stairs = default;
    [SerializeField] public Material hidePlate = default;
}