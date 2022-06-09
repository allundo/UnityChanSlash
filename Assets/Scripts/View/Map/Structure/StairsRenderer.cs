using UnityEngine;

public class StairsRenderer : StructuresRenderer<StairsControl>
{
    private StairsControl prefabUpStairsN;
    private StairsControl prefabDownStairsN;

    private Material floorMatStairs;
    private Material floorMatWall;

    public StairsRenderer(Transform parent) : base(parent)
    {
        prefabUpStairsN = Resources.Load<StairsControl>("Prefabs/Map/UpStairsN");
        prefabDownStairsN = Resources.Load<StairsControl>("Prefabs/Map/DownStairsN");
    }

    public override void LoadFloorMaterials(WorldMap map)
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
}