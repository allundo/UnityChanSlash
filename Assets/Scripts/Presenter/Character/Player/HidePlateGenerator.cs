using UnityEngine;

public class HidePlateGenerator : MobGenerator<HidePlate>
{
    private float[] angle = new float[0b10000];

    protected override void Awake()
    {
        base.Awake();

        angle[(int)Plate.A] = angle[(int)Plate.AB] = angle[(int)Plate.ABC] = angle[(int)Plate.ABCD] = angle[(int)Plate.AD] = 0f;
        angle[(int)Plate.B] = angle[(int)Plate.BD] = angle[(int)Plate.ABD] = angle[(int)Plate.BC] = 90f;
        angle[(int)Plate.D] = angle[(int)Plate.CD] = angle[(int)Plate.BCD] = 180f;
        angle[(int)Plate.C] = angle[(int)Plate.AC] = angle[(int)Plate.ACD] = -90f;
    }

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

    public HidePlate Spawn(Vector3 offset, Plate plate, float duration = 0.01f)
    {
        float rotateAngle = angle[(int)plate];
        return base.Spawn(offset, Quaternion.Euler(0, rotateAngle, 0), duration).SetPlateType(plate, rotateAngle);
    }

    protected override HidePlate GetInstance(HidePlate prefab)
    {
        return GetPooledObj() ?? Instantiate(prefab, pool, false).SetMaterial(material);
    }
}
