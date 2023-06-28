using UnityEngine;

public class PitTrapsRenderer : StructuresRenderer<PitControl>
{
    private PitControl prefabPitTrap;

    public PitTrapsRenderer(Transform parent) : base(parent)
    {
        prefabPitTrap = Resources.Load<PitControl>("Prefabs/Map/PitTrap");
    }

    public void SetPitTrap(Pos pos)
    {
        PlacePrefab(pos, prefabPitTrap).SetMaterials(floorMaterials.pitLid, floorMaterials.wall).SetPitState(map.GetTile(pos) as Pit);
    }
}
