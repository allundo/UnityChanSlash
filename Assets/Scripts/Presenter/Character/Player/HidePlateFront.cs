using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class HidePlateFront : MonoBehaviour
{
    /// <summary>
    /// Quaternion rotations applies to PlateFront for each direction.
    /// </summary>
    protected Dictionary<IDirection, Quaternion> rotate = new Dictionary<IDirection, Quaternion>();

    protected Dictionary<IDirection, Vector4> texRotateMatrix = new Dictionary<IDirection, Vector4>();

    /// <summary>
    /// Normalized tile vectors expresses direction.
    /// </summary>
    protected Dictionary<IDirection, Pos> vec = new Dictionary<IDirection, Pos>();

    /// <summary>
    /// Current tile map offset position of PlateFront from player position.
    /// </summary>
    private Pos currentOffset = new Pos();

    /// <summary>
    /// Current rotation of PlateFront decided by player's direction.
    /// </summary>
    private Quaternion currentRotation = Quaternion.identity;

    private Vector4 currentTexRotate = new Vector4(1, 0, 0, 1);

    private Material[] floorMaterials;

    protected WorldMap map;
    protected Renderer plateRenderer;
    protected Material material;

    protected int Range = 11;
    protected int Width = 7;
    protected int Height = 15;

    void Awake()
    {
        plateRenderer = GetComponent<Renderer>();
        map = GameManager.Instance.worldMap;

        rotate[Direction.north] = Quaternion.identity;
        rotate[Direction.east] = Quaternion.Euler(0f, 90f, 0f);
        rotate[Direction.south] = Quaternion.identity;
        rotate[Direction.west] = Quaternion.Euler(0f, 90f, 0f);

        var idMat = new Vector4(1, 0, 0, 1);
        var rewindMat = new Vector4(0, -1, 1, 0);

        texRotateMatrix[Direction.north] = idMat;
        texRotateMatrix[Direction.east] = rewindMat;
        texRotateMatrix[Direction.south] = idMat;
        texRotateMatrix[Direction.west] = rewindMat;

        vec[Direction.north] = new Pos(0, -1);
        vec[Direction.east] = new Pos(1, 0);
        vec[Direction.south] = new Pos(0, 1);
        vec[Direction.west] = new Pos(-1, 0);
    }

    public HidePlateFront SetFloorMaterials(FloorMaterialsData data)
    {
        floorMaterials = new Material[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            floorMaterials[i] = data.Param(i).plateFront;
        }
        return this;
    }

    public void InitPlateSize(int range, int width, int height)
    {
        this.Range = range;
        this.Width = width;
        this.Height = height;
    }
    public void Move(Pos pos)
    {
        // Move with delay
        DOVirtual.DelayedCall(0.1f, () =>
        {
            transform.rotation = currentRotation;
            transform.position = map.WorldPos(pos + currentOffset);
            Debug.Log("GetVector: " + material.GetVector("_Rotate"));
            Debug.Log("CurrentTexRotate: " + currentTexRotate);
            material.SetVector("_Rotate", currentTexRotate);
        })
        .Play();
    }

    public void SwitchWorldMap(WorldMap map)
    {
        Util.SwitchMaterial(plateRenderer, floorMaterials[map.floor - 1]);
        material = plateRenderer.sharedMaterial;
        this.map = map;
    }

    public void SetPortraitOffset(IDirection dir)
    {
        currentOffset = vec[dir] * (2 * Height - Width);
    }

    public void SetLandscapeOffset(IDirection dir)
    {
        currentOffset = vec[dir] * (Range * 3 / 2 + 1);
    }

    public void SetRotation(IDirection dir)
    {
        currentRotation = rotate[dir];
        currentTexRotate = texRotateMatrix[dir];
    }
}