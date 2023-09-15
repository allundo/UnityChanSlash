using UnityEngine;

public class CustomStructureRenderer : ObjectsRenderer<GameObject>
{
    protected FloorCustomStructure floorCustomStructure;

    public CustomStructureRenderer(Transform parent) : base(parent) { }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorCustomStructure = ResourceLoader.Instance.floorCustomStructureData.Param(map.floor - 1);
    }

    public void SetCustomStructures(Dir[,] dirMap)
    {
        var csPos = map.customStructurePos;
        for (int i = 0; i < csPos.Length; ++i)
        {
            Pos pos = csPos[i];
            PlacePrefab(pos, floorCustomStructure.prefabStructures[i], Direction.Convert(dirMap[pos.x, pos.y]));
        }
    }
}
