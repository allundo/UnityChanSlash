using System;
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
    [SerializeField] private GameObject plate5 = default;
    [SerializeField] private GameObject plate4 = default;
    [SerializeField] private GameObject plate3 = default;
    [SerializeField] private GameObject plate2 = default;
    [SerializeField] private GameObject plate1 = default;

    private Mesh[] wallMesh = new Mesh[0b10000];
    private Mesh[] gateMesh = new Mesh[0b10000];
    private Mesh wallVMesh;
    private Mesh wallHMesh;
    private GameObject[] plate = new GameObject[0b10000];
    private HidePool hidePool;

    private Mesh GetMeshFromObject(GameObject go) => go.GetComponent<MeshFilter>().sharedMesh;
    public void Init(WorldMap map)
    {
        this.map = map;
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

        hidePool = new HidePool(map, plate5, plate4, plate3, plate2, plate1);
    }

    private void SetDoor(Pos pos, GameObject doorPrefab)
    {
        GameObject door = PlacePrefab(pos, doorPrefab);

        Door tileDoor = (Door)GameManager.Instance.worldMap.GetTile(pos);
        tileDoor.dc = door.GetComponent<DoorControl>();
    }

    private GameObject PlacePrefab(Pos pos, GameObject prefab)
    {
        return Instantiate(prefab, WorldPos(pos), Quaternion.identity);
    }

    public Vector3 WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public Vector3 WorldPos(int x, int y) => map.WorldPos(x, y);

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
                        if (dirMap[i, j] == Dir.NS) SetDoor(new Pos(i, j), doorV);
                        if (dirMap[i, j] == Dir.EW) SetDoor(new Pos(i, j), doorH);
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
        return new CombineInstance()
        {
            mesh = src,
            transform = Matrix4x4.Translate(WorldPos(pos))
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

    public void RedrawHidePlates(Vector3 playerPos) => hidePool.RedrawHidePlates(playerPos);
    public void MoveHidePlates(Vector3 playerPos) => hidePool.MoveHidePlates(playerPos);

}
