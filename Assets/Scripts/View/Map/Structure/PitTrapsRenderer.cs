using UnityEngine;

public class PitTrapsRenderer : StructuresRenderer<PitControl>
{
    private PitControl prefabPitTrap;

    private Material floorMatPitLid;
    private Material floorMatWall;
    private float floorPitDamage;

    public PitTrapsRenderer(Transform parent) : base(parent)
    {
        prefabPitTrap = Resources.Load<PitControl>("Prefabs/Map/PitTrap");
    }

    public override void LoadFloorMaterials(WorldMap map)
    {
        this.map = map;
        var floorMaterials = floorMaterialsData.Param(map.floor - 1);
        floorMatPitLid = floorMaterials.pitLid;
        floorMatWall = floorMaterials.wall;
        floorPitDamage = floorMaterials.pitDamage;
    }


    public void SetPitTrap(Pos pos)
    {
        var state = new PitState(floorPitDamage);

        PlacePrefab(pos, prefabPitTrap).SetState(state).SetMaterials(floorMatPitLid, floorMatWall);
        (map.GetTile(pos) as Pit).state = state;
    }
}