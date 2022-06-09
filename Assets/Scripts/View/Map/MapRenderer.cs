using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MapRenderer : MonoBehaviour
{
    private FloorMaterialsData floorMaterialsData;
    private FloorMaterialsSource floorMaterials;

    private FloorMessagesData floorMessagesData;
    private FloorMessagesSource floorMessages;

    private MessageData[] FixedMessage(int index) => floorMessages.fixedMessages[index].Convert();
    private MessageData[] RandomMessage() => floorMessages.randomMessages[Random.Range(0, floorMessages.randomMessages.Length)].Convert();

    private List<Pos>[] tileOpenData;

    public WorldMap map { get; private set; }

    // Instantiated GameObjects
    [SerializeField] private GameObject wallParent = default;
    [SerializeField] private GameObject wallV = default;
    [SerializeField] private GameObject wallH = default;
    [SerializeField] private GameObject pillarNextWallV = default;
    [SerializeField] private GameObject pillarNextWallH = default;
    [SerializeField] private GameObject pillarNextWallN = default;
    [SerializeField] private GameObject pillarNextWallE = default;
    [SerializeField] private GameObject pillarNextWallS = default;
    [SerializeField] private GameObject pillarNextWallW = default;
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
    [SerializeField] private GameObject pillar = default;
    [SerializeField] private GameObject ground = default;

    // Prefabs
    [SerializeField] private StairsControl upStairsN = default;
    [SerializeField] private StairsControl downStairsN = default;
    [SerializeField] private GameObject messageBoardN = default;
    [SerializeField] private BoxControl treasureBoxN = default;
    [SerializeField] private PitControl pitTrap = default;

    private Mesh[] wallMesh = new Mesh[0b10000];
    private Mesh[] gateMesh = new Mesh[0b10000];

    private Mesh wallVMesh;
    private Mesh wallHMesh;

    private Mesh GetMeshFromObject(GameObject go) => go.GetComponent<MeshFilter>().sharedMesh;

    private DoorsRenderer doorsRenderer;

    void Awake()
    {
        floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;
        floorMessagesData = ResourceLoader.Instance.floorMessagesData;

        tileOpenData = new List<Pos>[floorMaterialsData.Length].Select(_ => new List<Pos>()).ToArray();

        doorsRenderer = new DoorsRenderer(transform);
    }

    ///  <summary>
    /// Initiate meshes to combine. This method must be called before rendering.
    /// </summary>
    public void InitMeshes()
    {
        wallVMesh = GetMeshFromObject(wallV);
        wallHMesh = GetMeshFromObject(wallH);

        wallMesh[(int)Dir.N] = GetMeshFromObject(pillarNextWallN);
        wallMesh[(int)Dir.E] = GetMeshFromObject(pillarNextWallE);
        wallMesh[(int)Dir.S] = GetMeshFromObject(pillarNextWallS);
        wallMesh[(int)Dir.W] = GetMeshFromObject(pillarNextWallW);
        wallMesh[(int)Dir.NS] = GetMeshFromObject(pillarNextWallV);
        wallMesh[(int)Dir.EW] = GetMeshFromObject(pillarNextWallH);

        gateMesh[(int)Dir.NONE] = GetMeshFromObject(pillar);
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

    private void SetMessageBoard(Pos pos, IDirection dir)
    {
        PlacePrefab(pos, messageBoardN, dir.Rotate);
        MessageWall tile = map.GetTile(pos) as MessageWall;
        tile.boardDir = dir;

        if (tile.Read == null)
        {
            int fixedIndex = map.fixedMessagePos.IndexOf(pos);
            tile.data = fixedIndex != -1 ? FixedMessage(fixedIndex) : RandomMessage();
        }
    }

    private void SetStairs(Pos pos, IDirection dir, bool isDownStairs)
    {
        PlacePrefab(pos, isDownStairs ? downStairsN : upStairsN, dir.Rotate)
            .SetMaterials(floorMaterials.stairs, isDownStairs ? floorMaterials.wall : null);

        Stairs tileStairs = map.GetTile(pos) as Stairs;
        tileStairs.enterDir = dir;
        tileStairs.isDownStairs = isDownStairs;
    }

    private void SetPitTrap(Pos pos)
    {
        var state = new PitState(floorMaterials.pitDamage);

        PlacePrefab(pos, pitTrap).SetState(state).SetMaterials(floorMaterials.pitLid, floorMaterials.wall);
        (map.GetTile(pos) as Pit).state = state;
    }

    private void SetBox(Pos pos, IDirection dir)
    {
        var state = new BoxState();
        var box = PlacePrefab(pos, treasureBoxN, dir.Rotate).SetState(state) as BoxControl;
        (map.GetTile(pos) as Box).state = state;

        boxesPool.Push(box);
    }

    private Stack<GameObject> objectPool = new Stack<GameObject>();
    private Stack<BoxControl> boxesPool = new Stack<BoxControl>();

    // For prefab components
    private T PlacePrefab<T>(Pos pos, T prefab) where T : MonoBehaviour
        => PlacePrefab(pos, prefab, Quaternion.identity);
    private T PlacePrefab<T>(Pos pos, T prefab, Quaternion rotation) where T : MonoBehaviour
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, transform);
        objectPool.Push(instance.gameObject); // MonoBehaviour components cannot be CASTed to GameObject by "as".
        return instance;
    }

    // For prefab GameObjects
    private GameObject PlacePrefab(Pos pos, GameObject prefab)
        => PlacePrefab(pos, prefab, Quaternion.identity) as GameObject;

    private GameObject PlacePrefab(Pos pos, GameObject prefab, Quaternion rotation)
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, transform);
        objectPool.Push(instance);
        return instance;
    }

    public void DestroyObjects()
    {
        doorsRenderer.DestroyObjects();

        boxesPool.ForEach(box => box.KillTween());
        boxesPool.Clear();

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
        floorMessages = floorMessagesData.Param(map.floor - 1);
        doorsRenderer.LoadFloorMaterials(map);
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
                Pos pos = new Pos(i, j);

                switch (matrix[i, j])
                {
                    case Terrain.Ground:
                        break;

                    case Terrain.Pillar:
                        terrainMeshes.Add(GetMeshInstance(gateMesh[(int)dirMap[i, j]], pos));
                        break;

                    case Terrain.MessagePillar:
                        // Render a message board
                        SetMessageBoard(pos, Direction.Convert(dirMap[i, j]));

                        // Render a pillar
                        terrainMeshes.Add(GetMeshInstance(gateMesh[(int)map.GetDoorDir(i, j)], pos));
                        break;

                    case Terrain.Door:
                        if (dirMap[i, j] == Dir.NS) doorsRenderer.SetDoorV(pos);
                        if (dirMap[i, j] == Dir.EW) doorsRenderer.SetDoorH(pos);
                        break;

                    case Terrain.ExitDoor:
                        doorsRenderer.SetExitDoor(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Wall:
                    case Terrain.MessageWall:
                        // Render a message board
                        if (matrix[i, j] == Terrain.MessageWall)
                        {
                            var dir = Direction.Convert(dirMap[i, j]);
                            SetMessageBoard(pos, dir);

                            // Change back Dir data for rendering wall
                            dirMap[i, j] = dir.Left.Enum | dir.Right.Enum;
                        }

                        // Render a wall
                        Dir pillarDir = map.GetPillarDir(i, j);
                        if (pillarDir == Dir.NONE)
                        {
                            if (dirMap[i, j] == Dir.NS) terrainMeshes.Add(GetMeshInstance(wallVMesh, pos));
                            if (dirMap[i, j] == Dir.EW) terrainMeshes.Add(GetMeshInstance(wallHMesh, pos));
                            break;
                        }
                        terrainMeshes.Add(GetMeshInstance(wallMesh[(int)pillarDir], pos));
                        break;

                    case Terrain.DownStairs:
                        SetStairs(pos, Direction.Convert(dirMap[i, j]), true);
                        break;

                    case Terrain.UpStairs:
                        SetStairs(pos, Direction.Convert(dirMap[i, j]), false);
                        break;

                    case Terrain.Box:
                        SetBox(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Pit:
                        SetPitTrap(pos);
                        break;
                }
            }
        }
        return terrainMeshes;
    }

    public void StoreMapData()
    {
        var store = tileOpenData[this.map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            if (tile is IOpenable && (tile as IOpenable).IsOpen) store.Add(pos);
        });
    }

    public void RestoreMapData(WorldMap map)
    {
        var restore = tileOpenData[map.floor - 1];
        restore.ForEach(pos => (map.GetTile(pos) as IOpenable).Open());
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
