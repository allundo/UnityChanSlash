using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] private FloorMaterialsData floorMaterialsData = default;
    private FloorMaterialsSource floorMaterials;

    private List<Pos>[] doorOpenData;

    public WorldMap map { get; private set; }

    [SerializeField] private GameObject wallParent = default;
    [SerializeField] private GameObject wallV = default;
    [SerializeField] private GameObject wallH = default;
    [SerializeField] private GameObject pallNextWallV = default;
    [SerializeField] private GameObject pallNextWallH = default;
    [SerializeField] private GameObject pallNextWallN = default;
    [SerializeField] private GameObject pallNextWallE = default;
    [SerializeField] private GameObject pallNextWallS = default;
    [SerializeField] private GameObject pallNextWallW = default;
    [SerializeField] private GameObject gateV = default;
    [SerializeField] private GameObject gateH = default;
    [SerializeField] private GameObject gateVE = default;
    [SerializeField] private GameObject gateVW = default;
    [SerializeField] private GameObject gateHN = default;
    [SerializeField] private GameObject gateHS = default;
    [SerializeField] private GameObject gateCross = default;
    [SerializeField] private GameObject gateNE = default;
    [SerializeField] private GameObject gateES = default;
    [SerializeField] private GameObject gateSW = default;
    [SerializeField] private GameObject gateWN = default;
    [SerializeField] private GameObject gateN = default;
    [SerializeField] private GameObject gateE = default;
    [SerializeField] private GameObject gateS = default;
    [SerializeField] private GameObject gateW = default;
    [SerializeField] private GameObject pall = default;
    [SerializeField] private DoorControl doorV = default;
    [SerializeField] private DoorControl doorH = default;
    [SerializeField] private ExitDoorControl exitDoorN = default;
    [SerializeField] private StairsControl upStairsN = default;
    [SerializeField] private StairsControl downStairsN = default;
    [SerializeField] private GameObject ground = default;

    private Mesh[] wallMesh = new Mesh[0b10000];
    private Mesh[] gateMesh = new Mesh[0b10000];

    private Mesh wallVMesh;
    private Mesh wallHMesh;

    private Mesh GetMeshFromObject(GameObject go) => go.GetComponent<MeshFilter>().sharedMesh;

    void Awake()
    {
        doorOpenData = new List<Pos>[floorMaterialsData.Length].Select(_ => new List<Pos>()).ToArray();
    }

    ///  <summary>
    /// Initiate meshes to combine. This method must be called before rendering.
    /// </summary>
    public void InitMeshes()
    {
        wallVMesh = GetMeshFromObject(wallV);
        wallHMesh = GetMeshFromObject(wallH);

        wallMesh[(int)Dir.N] = GetMeshFromObject(pallNextWallN);
        wallMesh[(int)Dir.E] = GetMeshFromObject(pallNextWallE);
        wallMesh[(int)Dir.S] = GetMeshFromObject(pallNextWallS);
        wallMesh[(int)Dir.W] = GetMeshFromObject(pallNextWallW);
        wallMesh[(int)Dir.NS] = GetMeshFromObject(pallNextWallV);
        wallMesh[(int)Dir.EW] = GetMeshFromObject(pallNextWallH);

        gateMesh[(int)Dir.NONE] = GetMeshFromObject(pall);
        gateMesh[(int)Dir.N] = GetMeshFromObject(gateN);
        gateMesh[(int)Dir.E] = GetMeshFromObject(gateE);
        gateMesh[(int)Dir.S] = GetMeshFromObject(gateS);
        gateMesh[(int)Dir.W] = GetMeshFromObject(gateW);
        gateMesh[(int)Dir.NE] = GetMeshFromObject(gateNE);
        gateMesh[(int)Dir.ES] = GetMeshFromObject(gateES);
        gateMesh[(int)Dir.SW] = GetMeshFromObject(gateSW);
        gateMesh[(int)Dir.WN] = GetMeshFromObject(gateWN);
        gateMesh[(int)Dir.NS] = GetMeshFromObject(gateV);
        gateMesh[(int)Dir.EW] = GetMeshFromObject(gateH);
        gateMesh[(int)Dir.NES] = GetMeshFromObject(gateVE);
        gateMesh[(int)Dir.ESW] = GetMeshFromObject(gateHS);
        gateMesh[(int)Dir.SWN] = GetMeshFromObject(gateVW);
        gateMesh[(int)Dir.WNE] = GetMeshFromObject(gateHN);
        gateMesh[(int)Dir.NESW] = GetMeshFromObject(gateCross);
    }

    private void SetDoor(Pos pos, DoorControl doorPrefab)
    {
        InitAndStoreDoorData(pos, PlacePrefab(pos, doorPrefab));
    }

    private void SetExitDoor(Pos pos, IDirection dir)
    {
        InitAndStoreDoorData(pos, PlacePrefab(pos, exitDoorN, dir.Rotate).SetDir(dir));
    }

    private void InitAndStoreDoorData(Pos pos, DoorControl doorInstance)
    {
        var state = new DoorState(doorInstance.LockType);

        doorInstance.SetMaterials(floorMaterials.gate, floorMaterials.door).SetState(state);
        (map.GetTile(pos) as Door).state = state;

        doorsPool.Push(doorInstance);
    }

    public void SetStairs(Pos pos, IDirection dir, bool isDownStairs)
    {
        var stairs = PlacePrefab(pos, isDownStairs ? downStairsN : upStairsN, dir.Rotate);

        Stairs tileStairs = map.GetTile(pos) as Stairs;
        tileStairs.enterDir = dir;
        tileStairs.isDownStairs = isDownStairs;

        stairs.GetComponent<StairsControl>().SetMaterials(floorMaterials.stairs, isDownStairs ? floorMaterials.wall : null);
    }

    private Stack<GameObject> objectPool = new Stack<GameObject>();
    private Stack<DoorControl> doorsPool = new Stack<DoorControl>();

    private T PlacePrefab<T>(Pos pos, T prefab) where T : UnityEngine.Object
        => PlacePrefab(pos, prefab, Quaternion.identity);
    private T PlacePrefab<T>(Pos pos, T prefab, Quaternion rotation) where T : UnityEngine.Object
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, transform);
        objectPool.Push(instance as GameObject);
        return instance;
    }

    public void DestroyObjects()
    {
        doorsPool.ForEach(door => door.KillTween());
        doorsPool.Clear();

        objectPool.ForEach(obj => Destroy(obj));
        objectPool.Clear();
    }

    public void Render(WorldMap map)
    {
        SetActiveTerrains(false);
        DestroyObjects();
        LoadFloorMaterials(map);
        InitMeshes();
        GenerateTerrain(SetUpTerrainMeshes(map));
        SwitchTerrainMaterials(map);
        SetActiveTerrains(true);
    }

    public void LoadFloorMaterials(WorldMap map)
    {
        this.map = map;
        floorMaterials = floorMaterialsData.Param(map.floor - 1);
    }

    public List<CombineInstance> SetUpTerrainMeshes(WorldMap map)
    {
        int width = map.Width;
        int height = map.Height;

        var dirMap = map.CloneDirMap();
        var matrix = map.CloneMatrix();

        var terrainMeshes = new List<CombineInstance>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                switch (matrix[i, j])
                {
                    case Terrain.Ground:
                        break;

                    case Terrain.Pall:
                        terrainMeshes.Add(GetMeshInstance(gateMesh[(int)dirMap[i, j]], new Pos(i, j)));
                        break;

                    case Terrain.Door:
                        if (dirMap[i, j] == Dir.NS) SetDoor(new Pos(i, j), doorV);
                        if (dirMap[i, j] == Dir.EW) SetDoor(new Pos(i, j), doorH);
                        break;

                    case Terrain.ExitDoor:
                        SetExitDoor(new Pos(i, j), Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Wall:
                        Dir pallDir = map.GetPallDir(i, j);
                        if (pallDir == Dir.NONE)
                        {
                            if (dirMap[i, j] == Dir.NS) terrainMeshes.Add(GetMeshInstance(wallVMesh, new Pos(i, j)));
                            if (dirMap[i, j] == Dir.EW) terrainMeshes.Add(GetMeshInstance(wallHMesh, new Pos(i, j)));
                            break;
                        }
                        terrainMeshes.Add(GetMeshInstance(wallMesh[(int)pallDir], new Pos(i, j)));
                        break;

                    case Terrain.DownStairs:
                        SetStairs(new Pos(i, j), Direction.Convert(dirMap[i, j]), true);
                        break;

                    case Terrain.UpStairs:
                        SetStairs(new Pos(i, j), Direction.Convert(dirMap[i, j]), false);
                        break;
                }
            }
        }
        return terrainMeshes;
    }

    public void SwitchWorldMap(WorldMap map)
    {
        var store = doorOpenData[this.map.floor - 1];
        var restore = doorOpenData[map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            if (tile is Door && (tile as Door).IsOpen) store.Add(pos);
        });

        Render(map);

        restore.ForEach(pos => (map.GetTile(pos) as Door).Handle());
        restore.Clear();
    }

    public void StoreMapData()
    {
        var store = doorOpenData[this.map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            if (tile is Door && (tile as Door).IsOpen) store.Add(pos);
        });
    }

    public void RestoreMapData(WorldMap map)
    {
        var restore = doorOpenData[map.floor - 1];
        restore.ForEach(pos => (map.GetTile(pos) as Door).Handle());
        restore.Clear();
    }

    private CombineInstance GetMeshInstance(Mesh src, Pos pos)
    {
        return new CombineInstance()
        {
            mesh = src,
            transform = Matrix4x4.Translate(map.WorldPos(pos))
        };
    }

    public void GenerateTerrain(List<CombineInstance> meshes)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "Wall";
        combinedMesh.CombineMeshes(meshes.ToArray(), true);

        // ProBuilder managed Mesh needs MeshUtility.CopyTo() to apply another Mesh object
        MeshUtility.CopyTo(combinedMesh, wallParent.GetComponent<MeshFilter>().sharedMesh);

        Destroy(combinedMesh);
    }

    public void SwitchTerrainMaterials(WorldMap map)
    {
        Util.SwitchMaterial(ground.GetComponent<Renderer>(), floorMaterials.ground);
        Util.SwitchMaterial(wallParent.GetComponent<Renderer>(), floorMaterials.wall);
    }

    public void SetActiveTerrains(bool isActive)
    {
        wallParent.SetActive(isActive);
        ground.gameObject.SetActive(isActive);
    }
}
