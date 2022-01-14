using UnityEngine;

public class HidePlateGenerator : MobGenerator<HidePlate>
{
    private Material material;

    public HidePlateGenerator SetPrefab(HidePlate prefab)
    {
        fixedPrefab = prefab;
        return this;
    }

    public HidePlateGenerator SetMaterial(Material material)
    {
        this.material = material;
        pool.ForEach(t => t.GetComponent<HidePlate>().SetMaterial(material));
        return this;
    }

    protected override HidePlate GetInstance(HidePlate prefab)
    {
        return GetPooledObj() ?? Instantiate(prefab, pool, false).SetMaterial(material);
    }
}
