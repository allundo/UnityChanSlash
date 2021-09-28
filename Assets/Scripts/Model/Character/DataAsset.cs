using UnityEngine;
using System;

[CreateAssetMenu(fileName = "MobData", menuName = "ScriptableObjects/CreateMobParamAsset")]
public class MobData : DataAsset<MobParam> { }

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/CreatePlayerParamAsset")]
public class PlayerData : MobData { }

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

    [SerializeField] public float faceDamageMultiplier = 1.0f;

    [SerializeField] public float sideDamageMultiplier = 1.5f;

    [SerializeField] public float backDamageMultiplier = 2.0f;

    [SerializeField] public float attack = 1.0f;

    [SerializeField] public float shield = 0.0f;
    [SerializeField] public float armorMultiplier = 1.0f;

}
