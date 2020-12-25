using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MapRenderer : SingletonMonoBehaviour<MapRenderer>
{
    private WorldMap map;

    private float TILE_UNIT => WorldMap.TILE_UNIT;

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
    [SerializeField] private GameObject doorV = default;
    [SerializeField] private GameObject doorH = default;
    private Mesh[] wallMesh = new Mesh[0b10000];
    private Mesh[] gateMesh = new Mesh[0b10000];
    private Mesh doorVMesh;
    private Mesh doorHMesh;
    private Mesh wallVMesh;
    private Mesh wallHMesh;

    public void Init(WorldMap map)
    {
        this.map = map;
        wallVMesh = wallV.GetComponent<MeshFilter>().sharedMesh;
        wallHMesh = wallH.GetComponent<MeshFilter>().sharedMesh;

        doorVMesh = doorV.GetComponent<MeshFilter>().sharedMesh;
        doorHMesh = doorH.GetComponent<MeshFilter>().sharedMesh;

        wallMesh[(int)Dir.N] = pallNextWallN.GetComponent<MeshFilter>().sharedMesh;
        wallMesh[(int)Dir.E] = pallNextWallE.GetComponent<MeshFilter>().sharedMesh;
        wallMesh[(int)Dir.S] = pallNextWallS.GetComponent<MeshFilter>().sharedMesh;
        wallMesh[(int)Dir.W] = pallNextWallW.GetComponent<MeshFilter>().sharedMesh;
        wallMesh[(int)Dir.NS] = pallNextWallV.GetComponent<MeshFilter>().sharedMesh;
        wallMesh[(int)Dir.EW] = pallNextWallH.GetComponent<MeshFilter>().sharedMesh;

        gateMesh[(int)Dir.NONE] = pall.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.N] = gateN.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.E] = gateE.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.S] = gateS.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.W] = gateW.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.NE] = gateNE.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.ES] = gateES.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.SW] = gateSW.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.WN] = gateWN.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.NS] = gateV.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.EW] = gateH.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.NES] = gateVE.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.ESW] = gateHS.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.SWN] = gateVW.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.WNE] = gateHN.GetComponent<MeshFilter>().sharedMesh;
        gateMesh[(int)Dir.NESW] = gateCross.GetComponent<MeshFilter>().sharedMesh;
    }

    private void SetTerrain((float x, float z) pos, GameObject prefab)
    {
        Instantiate(prefab, new Vector3(pos.x, 0.0f, pos.z), Quaternion.identity);
    }
    public (float x, float z) WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public (float x, float z) WorldPos(int x, int y) => map.WorldPos(x, y);

    public void Fix(MazeCreator maze)
    {

        int width = maze.Width;
        int height = maze.Height;

        Dir[,] dirMap = (Dir[,])maze.GetDirMap().Clone();
        Terrain[,] matrix = (Terrain[,])maze.Matrix.Clone();

        var terrainMeshes = new Stack<CombineInstance>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                switch (matrix[i, j])
                {
                    case Terrain.Ground:
                        break;

                    case Terrain.Pall:
                        terrainMeshes.Push(GetMeshInstance(gateMesh[(int)dirMap[i, j]], new Pos(i, j)));
                        break;

                    case Terrain.Door:
                        // if (dirMap[i, j] == Dir.NS) terrainMeshes.Push(GetMeshInstance(doorVMesh, new Pos(i, j)));
                        // if (dirMap[i, j] == Dir.EW) terrainMeshes.Push(GetMeshInstance(doorHMesh, new Pos(i, j)));
                        if (dirMap[i, j] == Dir.NS) SetTerrain(WorldPos(i, j), doorV);
                        if (dirMap[i, j] == Dir.EW) SetTerrain(WorldPos(i, j), doorH);
                        break;

                    case Terrain.Wall:
                        Dir pallDir = maze.GetPallDir(i, j);
                        if (pallDir == Dir.NONE)
                        {
                            if (dirMap[i, j] == Dir.NS) terrainMeshes.Push(GetMeshInstance(wallVMesh, new Pos(i, j)));
                            if (dirMap[i, j] == Dir.EW) terrainMeshes.Push(GetMeshInstance(wallHMesh, new Pos(i, j)));
                            break;
                        }
                        terrainMeshes.Push(GetMeshInstance(wallMesh[(int)pallDir], new Pos(i, j)));
                        break;
                }
            }
        }
        GenerateTerrain(terrainMeshes);
    }

    private CombineInstance GetMeshInstance(Mesh src, Pos pos)
    {
        (float x, float z) worldPos = WorldPos(pos);

        return new CombineInstance()
        {
            mesh = src,
            transform = Matrix4x4.Translate(new Vector3(worldPos.x, 0.0f, worldPos.z))
        };
    }

    private void GenerateTerrain(Stack<CombineInstance> meshes)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "Wall";
        combinedMesh.CombineMeshes(meshes.ToArray(), true);

        // ProBuilder managed Mesh needs MeshUtility.CopyTo() to apply another Mesh object
        MeshUtility.CopyTo(combinedMesh, wallParent.GetComponent<MeshFilter>().sharedMesh);
        wallParent.SetActive(true);
    }

    private void GenerateWall(Stack<Pos> wallPos)
    {
        Mesh wallMesh = wallParent.GetComponent<MeshFilter>().sharedMesh;

        int count = wallPos.Count;
        CombineInstance[] combineInstances = new CombineInstance[count];

        for (int i = 0; i < count; i++)
        {
            combineInstances[i].mesh = wallMesh;
            (float x, float z) pos = WorldPos(wallPos.Pop());

            // FIXME: localScale of wallParent is already TILE_UNIT
            // FIXME: position.y of wallParent is already set by TILE_UNIT / 2
            Matrix4x4 mat = Matrix4x4.Translate(new Vector3(pos.x, 0.0f, pos.z));

            combineInstances[i].transform = mat;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "Wall";
        combinedMesh.CombineMeshes(combineInstances, true);

        // ProBuilder managed Mesh needs MeshUtility.CopyTo() to apply another Mesh object
        MeshUtility.CopyTo(combinedMesh, wallParent.GetComponent<MeshFilter>().sharedMesh);

        wallParent.SetActive(true);
    }
}