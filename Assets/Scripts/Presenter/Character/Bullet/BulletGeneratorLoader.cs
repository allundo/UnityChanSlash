using UnityEngine;
using System.Collections.Generic;
using System;

public class BulletGeneratorLoader : MonoBehaviour
{
    public Dictionary<BulletType, BulletGenerator> bulletGenerators { get; protected set; } = new Dictionary<BulletType, BulletGenerator>();

    void Awake()
    {
        var prefabBulletGenerator = Resources.Load<BulletGenerator>("Prefabs/Generator/BulletGenerator");
        var bulletData = Resources.Load<BulletData>("DataAssets/Character/BulletData");

        foreach (BulletType type in Enum.GetValues(typeof(BulletType)))
        {
            bulletGenerators[type] = Instantiate(prefabBulletGenerator, transform).Init(new GameObject("Bullet Pool: " + type), bulletData.Param((int)type));
        }
    }

    public void DestroyAll()
    {
        bulletGenerators.ForEach(kv => kv.Value.DestroyAll());
    }
}
