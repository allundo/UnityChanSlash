using UnityEngine;
using System.Collections.Generic;
using System;

public class BulletGeneratorLoader : MonoBehaviour
{
    [SerializeField] protected BulletData data;
    protected BulletGenerator prefabBulletGenerator;

    public Dictionary<BulletType, BulletGenerator> bulletGenerators { get; protected set; } = new Dictionary<BulletType, BulletGenerator>();

    void Awake()
    {
        prefabBulletGenerator = Resources.Load<BulletGenerator>("Prefabs/Generator/BulletGenerator");

        foreach (BulletType type in Enum.GetValues(typeof(BulletType)))
        {
            bulletGenerators[type] = Instantiate(prefabBulletGenerator, transform).Init(new GameObject("Bullet Pool: " + type), data.Param((int)type));
        }
    }

    public void DestroyAll()
    {
        bulletGenerators.ForEach(kv => kv.Value.DestroyAll());
    }
}
