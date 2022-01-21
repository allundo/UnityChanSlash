using UnityEngine;

public class HidePlateGenerator : MobGenerator<HidePlate>
{
    private Quaternion[] rotate = new Quaternion[0b10000];
    private Vector4[] rewindMatrix = new Vector4[0b10000];

    protected override void Awake()
    {
        base.Awake();

        rotate[(int)Plate.A] = rotate[(int)Plate.AB] = rotate[(int)Plate.ABC] = rotate[(int)Plate.ABCD] = rotate[(int)Plate.AD] = Quaternion.identity;
        rotate[(int)Plate.B] = rotate[(int)Plate.BD] = rotate[(int)Plate.ABD] = rotate[(int)Plate.BC] = Quaternion.Euler(0f, 90f, 0f);
        rotate[(int)Plate.D] = rotate[(int)Plate.CD] = rotate[(int)Plate.BCD] = Quaternion.Euler(0f, 180f, 0f);
        rotate[(int)Plate.C] = rotate[(int)Plate.AC] = rotate[(int)Plate.ACD] = Quaternion.Euler(0f, -90f, 0f);

        rewindMatrix[(int)Plate.A] = rewindMatrix[(int)Plate.AB] = rewindMatrix[(int)Plate.ABC] = rewindMatrix[(int)Plate.ABCD] = rewindMatrix[(int)Plate.AD] = new Vector4(1, 0, 0, 1);
        rewindMatrix[(int)Plate.B] = rewindMatrix[(int)Plate.BD] = rewindMatrix[(int)Plate.ABD] = rewindMatrix[(int)Plate.BC] = new Vector4(0, -1, 1, 0);
        rewindMatrix[(int)Plate.D] = rewindMatrix[(int)Plate.CD] = rewindMatrix[(int)Plate.BCD] = new Vector4(-1, 0, 0, -1);
        rewindMatrix[(int)Plate.C] = rewindMatrix[(int)Plate.AC] = rewindMatrix[(int)Plate.ACD] = new Vector4(0, 1, -1, 0);
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
        return base.Spawn(offset, rotate[(int)plate], duration).SetPlateType(plate, rewindMatrix[(int)plate]);
    }

    protected override HidePlate GetInstance(HidePlate prefab)
    {
        return GetPooledObj() ?? Instantiate(prefab, pool, false).SetMaterial(material);
    }
}
