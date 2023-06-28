using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MapRenderer : MonoBehaviour
{
    private FloorMaterialsData floorMaterialsData;
    private FloorMaterialsSource floorMaterials;

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

    private Mesh[] wallMesh = new Mesh[0b10000];
    private Mesh[] gateMesh = new Mesh[0b10000];

    private Mesh wallVMesh;
    private Mesh wallHMesh;

    private Mesh wallParentMesh;

    private Mesh GetMeshFromObject(GameObject go) => go.GetComponent<MeshFilter>().sharedMesh;

    private DoorsRenderer doorsRenderer;
    private StairsRenderer stairsRenderer;
    private PitTrapsRenderer pitTrapsRenderer;
    private BoxesRenderer boxesRenderer;
    private MessageBoardsRenderer messageBoardsRenderer;
    private DoorDestructFXRenderer doorDestructFXRenderer;
    private IObjectsRenderer[] renderers;

    void Awake()
    {
        floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;

        doorsRenderer = new DoorsRenderer(transform);
        stairsRenderer = new StairsRenderer(transform);
        pitTrapsRenderer = new PitTrapsRenderer(transform);
        boxesRenderer = new BoxesRenderer(transform);
        messageBoardsRenderer = new MessageBoardsRenderer(transform);
        doorDestructFXRenderer = new DoorDestructFXRenderer(transform);

        renderers = new IObjectsRenderer[] { doorsRenderer, stairsRenderer, pitTrapsRenderer, boxesRenderer, messageBoardsRenderer, doorDestructFXRenderer };
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

    public void DestroyObjects()
    {
        renderers.ForEach(renderer => renderer.DestroyObjects());
    }

    public void Render(WorldMap map)
    {
        SetActiveTerrains(false);
        DestroyObjects();
        LoadFloorMaterials(map);
        InitMeshes();
        GenerateTerrain(SetUpTerrainMeshes(map.dirMapHandler));
        SwitchTerrainMaterials();
        SetActiveTerrains(true);
    }

    public void LoadFloorMaterials(WorldMap map)
    {
        this.map = map;
        floorMaterials = floorMaterialsData.Param(map.floor - 1);
        renderers.ForEach(renderer => renderer.SwitchWorldMap(map));
    }

    public List<CombineInstance> SetUpTerrainMeshes(DirMapHandler map)
    {
        var dirMap = map.dirMap.Clone() as Dir[,];
        var matrix = map.matrix.Clone() as Terrain[,];

        var terrainMeshes = new List<CombineInstance>();

        for (int j = 0; j < map.height; j++)
        {
            for (int i = 0; i < map.width; i++)
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
                    case Terrain.BloodMessagePillar:
                        // Render a message
                        messageBoardsRenderer.SetMessage(pos, Direction.Convert(dirMap[i, j]), matrix[i, j]);

                        // Render a pillar
                        terrainMeshes.Add(GetMeshInstance(gateMesh[(int)map.rawMapData.GetDoorDir(i, j)], pos));
                        break;

                    case Terrain.Door:
                        if (dirMap[i, j] == Dir.NS) doorsRenderer.SetDoorV(pos);
                        if (dirMap[i, j] == Dir.EW) doorsRenderer.SetDoorH(pos);
                        break;

                    case Terrain.LockedDoor:
                        doorsRenderer.SetLockedDoor(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.ExitDoor:
                        doorsRenderer.SetExitDoor(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Wall:
                    case Terrain.MessageWall:
                    case Terrain.BloodMessageWall:
                        // Render a message
                        if (matrix[i, j] != Terrain.Wall)
                        {
                            var dir = Direction.Convert(dirMap[i, j]);
                            messageBoardsRenderer.SetMessage(pos, dir, matrix[i, j]);

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
                        stairsRenderer.SetDownStairs(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.UpStairs:
                        stairsRenderer.SetUpStairs(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Box:
                        boxesRenderer.SetBox(pos, Direction.Convert(dirMap[i, j]));
                        break;

                    case Terrain.Pit:
                        pitTrapsRenderer.SetPitTrap(pos);
                        break;
                }
            }
        }
        return terrainMeshes;
    }

    public void PlaceBox(Pos pos)
    {
        IDirection dir = Direction.Convert(map.SetTerrain(pos.x, pos.y, Terrain.Box));
        boxesRenderer.SetBox(pos, dir);
    }

    public void ApplyTileState()
    {
        map.ApplyTileState();

        doorsRenderer.CompleteTween();
        boxesRenderer.CompleteTween();
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
        wallParentMesh = wallParent.GetComponent<MeshFilter>().mesh;
        MeshUtility.CopyTo(combinedMesh, wallParentMesh);

        Destroy(combinedMesh);
    }

    public void SwitchTerrainMaterials()
    {
        Util.SwitchMaterial(ground.GetComponent<Renderer>(), floorMaterials.ground);
        Util.SwitchMaterial(wallParent.GetComponent<Renderer>(), floorMaterials.wall);
    }

    public void SetActiveTerrains(bool isActive)
    {
        wallParent.SetActive(isActive);
        ground.gameObject.SetActive(isActive);
    }

    private void OnDestroy()
    {
        Destroy(wallParentMesh);
    }

    public void DoorDestructionVFX(Vector3 pos, IDirection dir) => doorDestructFXRenderer.Spawn(pos, dir);
}
