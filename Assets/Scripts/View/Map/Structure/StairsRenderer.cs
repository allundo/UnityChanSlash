using UnityEngine;
using System.Collections.Generic;

public class StairsRenderer
{
    private WorldMap map;
    private FloorMaterialsData floorMaterialsData;

    private StairsControl prefabUpStairsN;
    private StairsControl prefabDownStairsN;

    private Transform parentTransform;

    private Stack<StairsControl> stairsPool = new Stack<StairsControl>();

    private Material floorMatStairs;
    private Material floorMatWall;

    public StairsRenderer(Transform parent)
    {
        this.parentTransform = parent;
        this.floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;

        prefabUpStairsN = Resources.Load<StairsControl>("Prefabs/Map/UpStairsN");
        prefabDownStairsN = Resources.Load<StairsControl>("Prefabs/Map/DownStairsN");
    }

    public virtual void LoadFloorMaterials(WorldMap map)
    {
        this.map = map;
        var floorMaterials = floorMaterialsData.Param(map.floor - 1);
        floorMatStairs = floorMaterials.stairs;
        floorMatWall = floorMaterials.wall;
    }

    public void SetStairs(Pos pos, IDirection dir, bool isDownStairs)
    {
        PlacePrefab(pos, isDownStairs ? prefabDownStairsN : prefabUpStairsN, dir.Rotate)
            .SetMaterials(floorMatStairs, isDownStairs ? floorMatWall : null);

        Stairs tileStairs = map.GetTile(pos) as Stairs;
        tileStairs.enterDir = dir;
        tileStairs.isDownStairs = isDownStairs;
    }

    private T PlacePrefab<T>(Pos pos, T prefab) where T : StairsControl
        => PlacePrefab(pos, prefab, Quaternion.identity);

    private T PlacePrefab<T>(Pos pos, T prefab, Quaternion rotation) where T : StairsControl
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, parentTransform);
        stairsPool.Push(instance);
        return instance;
    }

    public void DestroyObjects()
    {
        stairsPool.ForEach(stairs =>
        {
            Object.Destroy(stairs);
        });
    }
}