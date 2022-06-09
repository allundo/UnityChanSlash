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
        var state = new PitState(floorMaterials.pitDamage);

        PlacePrefab(pos, prefabPitTrap).SetState(state).SetMaterials(floorMaterials.pitLid, floorMaterials.wall);
        (map.GetTile(pos) as Pit).state = state;
    }
}