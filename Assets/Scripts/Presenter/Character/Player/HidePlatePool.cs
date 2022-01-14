using UnityEngine;

/// <summary>
/// Pool HidePlate objects and manage instaces
/// </summary>
public class HidePlatePool : MonoBehaviour
{
    [SerializeField] private HidePlateGenerator prefabPlateGenerator = default;
    [SerializeField] private HidePlate prefabHidePlate1 = default;
    [SerializeField] private HidePlate prefabHidePlate2 = default;
    [SerializeField] private HidePlate prefabHidePlate3 = default;
    [SerializeField] private HidePlate prefabHidePlateFull = default;
    [SerializeField] private HidePlate prefabHidePlateCross = default;
    [SerializeField] private GameObject plateFrontPrefab = default;
    [SerializeField] private FloorMaterialsData floorMaterialsData = default;

    private HidePlateGenerator plate1Generator;
    private HidePlateGenerator plate2Generator;
    private HidePlateGenerator plate3Generator;
    private HidePlateGenerator plateFullGenerator;
    private HidePlateGenerator plateCrossGenerator;
    private FloorMaterialsSource floorMaterialsSource;
    GameObject plateFront;

    private HidePlateGenerator[] generator = new HidePlateGenerator[0b10000];
    private Quaternion[] rotate = new Quaternion[0b10000];

    private WorldMap map;

    private FloorMaterialsSource FloorMaterialSource(WorldMap map) => floorMaterialsData.Param(map.floor - 1);

    public GameObject PlateFront(WorldMap map)
    {
        plateFront = plateFront ?? Instantiate(plateFrontPrefab, Vector3.zero, Quaternion.identity);
        Util.SwitchMaterial(plateFront.GetComponent<Renderer>(), FloorMaterialSource(map).plateFront);
        return plateFront;
    }

    void Awake()
    {
        map = GameManager.Instance.worldMap;
        Material mat = FloorMaterialSource(map).hidePlate;

        plate1Generator = generator[(int)Plate.A] = generator[(int)Plate.B] = generator[(int)Plate.D] = generator[(int)Plate.C] = InstantiateGenerator(prefabHidePlate1, mat);
        plate2Generator = generator[(int)Plate.AB] = generator[(int)Plate.BD] = generator[(int)Plate.CD] = generator[(int)Plate.AC] = InstantiateGenerator(prefabHidePlate2, mat);
        plate3Generator = generator[(int)Plate.ABD] = generator[(int)Plate.BCD] = generator[(int)Plate.ACD] = generator[(int)Plate.ABC] = InstantiateGenerator(prefabHidePlate3, mat);
        plateFullGenerator = generator[(int)Plate.ABCD] = InstantiateGenerator(prefabHidePlateFull, mat);
        plateCrossGenerator = generator[(int)Plate.AD] = generator[(int)Plate.BC] = InstantiateGenerator(prefabHidePlateCross, mat);
        generator[(int)Plate.NONE] = null;

        rotate[(int)Plate.A] = rotate[(int)Plate.AB] = rotate[(int)Plate.ABC] = rotate[(int)Plate.ABCD] = rotate[(int)Plate.AD] = Quaternion.identity;
        rotate[(int)Plate.B] = rotate[(int)Plate.BD] = rotate[(int)Plate.ABD] = rotate[(int)Plate.BC] = Quaternion.Euler(0, 90, 0);
        rotate[(int)Plate.D] = rotate[(int)Plate.CD] = rotate[(int)Plate.BCD] = Quaternion.Euler(0, 180, 0);
        rotate[(int)Plate.C] = rotate[(int)Plate.AC] = rotate[(int)Plate.ACD] = Quaternion.Euler(0, -90, 0);
    }

    private HidePlateGenerator InstantiateGenerator(HidePlate prefabHidePlate, Material mat)
    {
        return Instantiate(prefabPlateGenerator, transform)
            .SetPrefab(prefabHidePlate)
            .SetMaterial(mat);
    }

    /// <summary>
    /// Spawn a HidePlate by selected HidePlateGenerator.
    /// </summary>
    /// <param name="plate">Plate shape type to spawn</param>
    /// <param name="pos">Spawn tile map postion</param>
    /// <param name="duration">Fade-in duration at spawning</param>
    /// <returns>One of pooled HidePlate or newly instantiated if not pooled</returns>
    public HidePlate SpawnPlate(Plate plate, Pos pos, float duration = 0.01f)
    {
        return generator[(int)plate]?.Spawn(map.WorldPos(pos), rotate[(int)plate], duration)?.SetPlate(plate);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        Material mat = FloorMaterialSource(map).hidePlate;

        new HidePlateGenerator[] { plate1Generator, plate2Generator, plate3Generator, plateCrossGenerator, plateFullGenerator }
            .ForEach(generator => generator.SetMaterial(mat));
    }
}
