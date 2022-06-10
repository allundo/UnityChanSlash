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

    public void SetDownStairs(Pos pos, IDirection dir)
    {
        PlacePrefab(pos, prefabDownStairsN, dir.Rotate).SetMaterials(floorMaterials.stairs, floorMaterials.wall);
        (map.GetTile(pos) as Stairs).enterDir = dir;
    }
    public void SetUpStairs(Pos pos, IDirection dir)
    {
        PlacePrefab(pos, prefabUpStairsN, dir.Rotate).SetMaterials(floorMaterials.stairs, null);
        (map.GetTile(pos) as Stairs).enterDir = dir;
    }
}
