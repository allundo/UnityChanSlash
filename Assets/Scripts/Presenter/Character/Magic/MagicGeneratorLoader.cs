using UnityEngine;
using System.Collections.Generic;
using System;

public class MagicGeneratorLoader : MonoBehaviour
{
    public Dictionary<MagicType, MagicGenerator> magicGenerators { get; protected set; } = new Dictionary<MagicType, MagicGenerator>();

    void Awake()
    {
        var prefabMagicGenerator = Resources.Load<MagicGenerator>("Prefabs/Generator/MagicGenerator");
        var magicData = Resources.Load<MagicData>("DataAssets/Character/MagicData");

        foreach (MagicType type in Enum.GetValues(typeof(MagicType)))
        {
            magicGenerators[type] = Instantiate(prefabMagicGenerator, transform).Init(new GameObject("Magic Pool: " + type), magicData.Param((int)type));
        }
    }

    public void DestroyAll()
    {
        magicGenerators.ForEach(kv => kv.Value.DestroyAll());
    }
}
