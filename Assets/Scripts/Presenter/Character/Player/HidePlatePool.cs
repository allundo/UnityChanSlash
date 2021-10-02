using UnityEngine;

/// <summary>
/// Pool HidePlate objects and manage instaces
/// </summary>
public class HidePlatePool : MonoBehaviour
{
    [SerializeField] private HidePlateGenerator plate1Generator = default;
    [SerializeField] private HidePlateGenerator plate2Generator = default;
    [SerializeField] private HidePlateGenerator plate3Generator = default;
    [SerializeField] private HidePlateGenerator plateFullGenerator = default;
    [SerializeField] private HidePlateGenerator plateCrossGenerator = default;

    private HidePlateGenerator[] generator = new HidePlateGenerator[0b10000];
    private Quaternion[] rotate = new Quaternion[0b10000];

    private WorldMap map;

    void Awake()
    {
        map = GameManager.Instance.worldMap;

        generator[(int)Plate.A] = generator[(int)Plate.B] = generator[(int)Plate.D] = generator[(int)Plate.C] = plate1Generator;
        generator[(int)Plate.AB] = generator[(int)Plate.BD] = generator[(int)Plate.CD] = generator[(int)Plate.AC] = plate2Generator;
        generator[(int)Plate.ABD] = generator[(int)Plate.BCD] = generator[(int)Plate.ACD] = generator[(int)Plate.ABC] = plate3Generator;
        generator[(int)Plate.ABCD] = plateFullGenerator;
        generator[(int)Plate.AD] = generator[(int)Plate.BC] = plateCrossGenerator;
        generator[(int)Plate.NONE] = null;

        rotate[(int)Plate.A] = rotate[(int)Plate.AB] = rotate[(int)Plate.ABC] = rotate[(int)Plate.ABCD] = rotate[(int)Plate.AD] = Quaternion.identity;
        rotate[(int)Plate.B] = rotate[(int)Plate.BD] = rotate[(int)Plate.ABD] = rotate[(int)Plate.BC] = Quaternion.Euler(0, 90, 0);
        rotate[(int)Plate.D] = rotate[(int)Plate.CD] = rotate[(int)Plate.BCD] = Quaternion.Euler(0, 180, 0);
        rotate[(int)Plate.C] = rotate[(int)Plate.AC] = rotate[(int)Plate.ACD] = Quaternion.Euler(0, -90, 0);
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
}