using UnityEngine;

public class StairsRenderer : StructuresRenderer<StairsControl>
{
    private StairsControl prefabUpStairsN;
    private StairsControl prefabDownStairsN;

    public StairsRenderer(Transform parent) : base(parent)
    {
        prefabUpStairsN = Resources.Load<StairsControl>("Prefabs/Map/UpStairsN");
        prefabDownStairsN = Resources.Load<StairsControl>("Prefabs/Map/DownStairsN");
    }

    public void SetStairs(Pos pos, IDirection dir, bool isDownStairs)
    {
        PlacePrefab(pos, isDownStairs ? prefabDownStairsN : prefabUpStairsN, dir.Rotate)
            .SetMaterials(floorMaterials.stairs, isDownStairs ? floorMaterials.wall : null);

        Stairs tileStairs = map.GetTile(pos) as Stairs;
        tileStairs.enterDir = dir;
        tileStairs.isDownStairs = isDownStairs;
    }
}