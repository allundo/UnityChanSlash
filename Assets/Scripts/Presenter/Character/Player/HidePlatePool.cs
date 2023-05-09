using UnityEngine;

/// <summary>
/// Pool HidePlate objects and manage instances
/// </summary>
public class HidePlatePool : MonoBehaviour
{
    [SerializeField] private HidePlateGenerator prefabPlateGenerator = default;
    [SerializeField] private HidePlate prefabHidePlate1 = default;
    [SerializeField] private HidePlate prefabHidePlate2 = default;
    [SerializeField] private HidePlate prefabHidePlate3 = default;
    [SerializeField] private HidePlate prefabHidePlateFull = default;
    [SerializeField] private HidePlate prefabHidePlateCross = default;
    [SerializeField] private HidePlateFront plateFrontPrefab = default;

    private HidePlateGenerator plate1Generator;
    private HidePlateGenerator plate2Generator;
    private HidePlateGenerator plate3Generator;
    private HidePlateGenerator plateFullGenerator;
    private HidePlateGenerator plateCrossGenerator;
    public HidePlateFront plateFront { get; private set; }

    private HidePlateGenerator[] generator = new HidePlateGenerator[0b10000];

    private WorldMap map;

    private FloorMaterialsData floorMaterialsData;
    private FloorMaterialsSource FloorMaterialSource(WorldMap map) => floorMaterialsData.Param(map.floor - 1);

    void Awake()
    {
        map = GameManager.Instance.worldMap;

        floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;
        Material mat = FloorMaterialSource(map).hidePlate;

        plateFront = Instantiate(plateFrontPrefab, transform);
        plateFront.SetFloorMaterials(floorMaterialsData).SwitchWorldMap(map);

        plate1Generator = generator[(int)Plate.A] = generator[(int)Plate.B] = generator[(int)Plate.D] = generator[(int)Plate.C] = InstantiateGenerator(prefabHidePlate1, mat);
        plate2Generator = generator[(int)Plate.AB] = generator[(int)Plate.BD] = generator[(int)Plate.CD] = generator[(int)Plate.AC] = InstantiateGenerator(prefabHidePlate2, mat);
        plate3Generator = generator[(int)Plate.ABD] = generator[(int)Plate.BCD] = generator[(int)Plate.ACD] = generator[(int)Plate.ABC] = InstantiateGenerator(prefabHidePlate3, mat);
        plateFullGenerator = generator[(int)Plate.ABCD] = InstantiateGenerator(prefabHidePlateFull, mat);
        plateCrossGenerator = generator[(int)Plate.AD] = generator[(int)Plate.BC] = InstantiateGenerator(prefabHidePlateCross, mat);
        generator[(int)Plate.NONE] = null;
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
    /// <param name="pos">Spawn tile map position</param>
    /// <param name="duration">Fade-in duration at spawning</param>
    /// <returns>One of pooled HidePlate or newly instantiated if not pooled</returns>
    public HidePlate SpawnPlate(Plate plate, Pos pos, float duration = 0.01f)
    {
        return generator[(int)plate]?.Spawn(map.WorldPos(pos), plate, duration);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        Material mat = FloorMaterialSource(map).hidePlate;

        new HidePlateGenerator[] { plate1Generator, plate2Generator, plate3Generator, plateCrossGenerator, plateFullGenerator }
            .ForEach(generator => generator.SetMaterial(mat));

        plateFront.SwitchWorldMap(map);
    }
}
