using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MapRenderer : SingletonMonoBehaviour<MapRenderer>
{

    private WorldMap map;

    private float TILE_UNIT => WorldMap.TILE_UNIT;

    [SerializeField] private GameObject wallParent = default;
    [SerializeField] private GameObject doorPrefab = default;

    private void SetTerrain((float x, float z) pos, GameObject prefab)
    {
        Instantiate(prefab, new Vector3(pos.x, TILE_UNIT * 0.5f, pos.z), Quaternion.identity);
    }
    public (float x, float z) WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public (float x, float z) WorldPos(int x, int y) => map.WorldPos(x, y);

    public void Fix(WorldMap map)
    {
        this.map = map;

        int width = map.Width;
        int height = map.Height;

        Terrain[,] matrix = (Terrain[,])map.Matrix.Clone();

        Stack<Pos> wallPos = new Stack<Pos>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                switch (matrix[i, j])
                {
                    case Terrain.Ground:
                        break;

                    case Terrain.Wall:
                        wallPos.Push(new Pos(i, j));
                        break;

                    case Terrain.TmpWall:
                        matrix[i, j] = Terrain.Wall;
                        wallPos.Push(new Pos(i, j));
                        break;

                    case Terrain.None:
                        // matrix[i, j] = Terrain.Ground;
                        matrix[i, j] = Terrain.Wall;
                        wallPos.Push(new Pos(i, j));
                        break;

                    case Terrain.VDoor:
                        if (
                            matrix[i, j - 1] == Terrain.Wall
                            || matrix[i, j - 1] == Terrain.Wall
                            || matrix[i - 1, j] == Terrain.Ground
                            || matrix[i + 1, j] == Terrain.Ground
                        )
                        {
                            matrix[i, j] = Terrain.Ground;
                            break;
                        }
                        SetTerrain(WorldPos(i, j), doorPrefab);

                        break;

                    case Terrain.HDoor:
                        if (
                            matrix[i, j - 1] == Terrain.Ground
                            || matrix[i, j - 1] == Terrain.Ground
                            || matrix[i - 1, j] == Terrain.Wall
                            || matrix[i + 1, j] == Terrain.Wall
                        )
                        {
                            matrix[i, j] = Terrain.Ground;
                            break;
                        }
                        SetTerrain(WorldPos(i, j), doorPrefab);

                        break;
                }
            }
        }
        GenerateWall(wallPos);
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